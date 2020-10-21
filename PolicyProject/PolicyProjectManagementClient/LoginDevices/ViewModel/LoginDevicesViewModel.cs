using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using NotifyHelper;
using PolicyProjectManagementService;
using RelayCmd;

namespace PolicyProjectManagementClient
{
    public class LoginDevicesViewModel : Notifier
    {
        private readonly IPpsClientViewModel _ppsClientViewModel;
        private ObservableCollection<SelectedDevice> _selectedDeviceCollection;
        private LoginDataContract _selectedLogin;
        private Dictionary<long, long[]> _loginIdAndDeviceIdDct;

        public LoginDevicesViewModel(IPpsClientViewModel ppsClientViewModel)
        {
            _ppsClientViewModel = ppsClientViewModel;
            _ppsClientViewModel.OnEmbeddedCollectionRefreshed += _ppsClientViewModel_OnEmbeddedCollectionRefreshed;
            SaveLoginDevicesCmd = new RelayCommand(SaveLoginDevices);
            GetLoginIdAndDeviceIdDct();
            UpdateLoginDeviceCollection();

            if (LoginCollection != null && LoginCollection.Any())
                SelectedLogin = LoginCollection.FirstOrDefault();
        }

        public ICommand SaveLoginDevicesCmd { get; private set; }

        public ObservableCollection<LoginDataContract> LoginCollection
        {
            get { return _ppsClientViewModel.LoginCollection; }
            set
            {
                _ppsClientViewModel.LoginCollection = value;
                NotifyPropertyChanged(nameof(LoginCollection));
            }
        }

        public ObservableCollection<SelectedDevice> DeviceCollection
        {
            get { return _selectedDeviceCollection; }
            set
            {
                _selectedDeviceCollection = value;
                NotifyPropertyChanged(nameof(DeviceCollection));
            }
        }

        public LoginDataContract SelectedLogin
        {
            get { return _selectedLogin; }
            set
            {
                if (_selectedLogin == value)
                    return;

                _selectedLogin = value;

                if (_selectedLogin == null)
                    return;

                CheckDevice(value);
                NotifyPropertyChanged(nameof(SelectedLogin));
            }
        }

        private void _ppsClientViewModel_OnEmbeddedCollectionRefreshed(string collectionName)
        {
            NotifyPropertyChanged(collectionName);

            if (collectionName == nameof(DeviceCollection))
                UpdateLoginDeviceCollection();
        }

        private void GetLoginIdAndDeviceIdDct()
        {
            try
            {
                using (var policySetService =
                    _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<ILoginDevicesService>())
                {
                    var channel = policySetService.CreateChannel();
                    var result = channel.GetLoginIdAndDeviceIdDct();

                    if (!result.BoolRes)
                        throw new Exception(result.ErrorRes);

                    _loginIdAndDeviceIdDct = result.SomeResult;
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage(string.Concat("Что-то пошло не так - ", ex.Message));
            }
        }

        private void UpdateLoginDeviceCollection()
        {
            try
            {
                if (_ppsClientViewModel.DeviceCollection == null || !_ppsClientViewModel.DeviceCollection.Any())
                    return;

                DeviceCollection = new ObservableCollection<SelectedDevice>();

                foreach (var device in _ppsClientViewModel.DeviceCollection)
                    DeviceCollection.Add(new SelectedDevice
                    {
                        DeviceId = device.DeviceId,
                        Selected = false,
                        DeviceName = device.DeviceName,
                        DevicePlatformName = device.DevicePlatformName
                    });
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage(string.Concat("Не удалось получить список устройств - ", ex.Message));
            }
        }

        protected void selectedDevice_OnDeviceSelected(long deviceId)
        {
            try
            {
                foreach (var device in DeviceCollection.Where(dev => dev.DeviceId != deviceId && dev.Selected))
                    device.Selected = false;
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage(string.Concat("Не удалось изменить выбор устройства - ",
                    ex.Message));
            }
        }

        private void CheckDevice(LoginDataContract selectedLogin)
        {
            try
            {
                if (DeviceCollection == null || !DeviceCollection.Any())
                    return;

                foreach (var selectedDevice in DeviceCollection)
                    selectedDevice.Selected = selectedDevice.DeviceTaken = false;

                using (var policySetService =
                    _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<ILoginDevicesService>())
                {
                    var channel = policySetService.CreateChannel();

                    if (selectedLogin == null)
                        return;

                    var result = channel.GetLoginDevices(null);

                    if (result == null)
                        throw new Exception("Ошибка вызова службы");

                    if (!result.BoolRes || result.SomeResult == null)
                        throw new Exception(result.ErrorRes);

                    var selectedDevices = result.SomeResult.Where(x => x.LoginId == selectedLogin.LoginId);

                    foreach (var selectedDevice in DeviceCollection)
                    {
                        var findInLoginDevice = selectedDevices.FirstOrDefault(x => x.DeviceId == selectedDevice.DeviceId);
                        selectedDevice.Selected = findInLoginDevice != null;
                        var loginDevice = result.SomeResult.FirstOrDefault(x => x.DeviceId == selectedDevice.DeviceId);
                        selectedDevice.NeedUpdateDevice = loginDevice != null && findInLoginDevice != null &&
                                                          (findInLoginDevice.NeedUpdateDevice ?? false);
                    }

                    if (_loginIdAndDeviceIdDct == null || !_loginIdAndDeviceIdDct.Any())
                        return;

                    var devLst = new List<long>();

                    foreach (
                        var devId in
                        _loginIdAndDeviceIdDct.Where(x => x.Key != selectedLogin.LoginId)
                            .SelectMany(set => set.Value.Where(devId => !devLst.Contains(devId))))
                        devLst.Add(devId);

                    foreach (var selectedDevice in DeviceCollection)
                    {
                        selectedDevice.DeviceTaken = devLst.Any(x => x == selectedDevice.DeviceId);
                        selectedDevice.OnDeviceSelected += selectedDevice_OnDeviceSelected;
                    }
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage(string.Concat("Не удалось выбрать устройства пользователя - ",
                    ex.Message));
            }
        }

        private void SaveLoginDevices(object parameter)
        {
            try
            {
                if (SelectedLogin == null)
                    throw new Exception("Пользователь не выбран");

                using (var policySetService =
                    _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<ILoginDevicesService>())
                {
                    var channel = policySetService.CreateChannel();
                    var result = channel.UpdateLoginDevices(SelectedLogin.LoginId,
                        DeviceCollection.Where(x => x.Selected)
                            .Select(
                                x =>
                                    new LoginDevicesDataContract
                                    {
                                        DeviceId = x.DeviceId,
                                        LoginId = SelectedLogin.LoginId,
                                        NeedUpdateDevice = true
                                    })
                            .ToArray());
                    GetLoginIdAndDeviceIdDct();

                    if (!result.BoolRes)
                        throw new Exception(result.ErrorRes);

                    _ppsClientViewModel.AddEvent(EventConstants.EVENT_ASSIGN_lOGIN_DEVICE,
                        string.Concat("Для логина ", SelectedLogin.Login));
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage(string.Concat("Не удалось сохранить устройства пользователя. ",
                    ex.Message));
            }
        }
    }
}