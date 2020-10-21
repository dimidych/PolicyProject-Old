using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using NotifyHelper;
using PolicyProjectManagementService;
using RelayCmd;

namespace PolicyProjectManagementClient
{
    public class DeviceViewModel : Notifier
    {
        private readonly IPpsClientViewModel _ppsClientViewModel;
        private DeviceInfoDataContract _selectedDevice;
        private PlatformDataContract _selectedPlatform;
        private string _deviceIpAddress;
        private string _deviceMacAddress;
        private string _deviceName;
        private string _deviceSerialNumber;
        private short _platformId;

        public DeviceViewModel(IPpsClientViewModel ppsClientViewModel)
        {
            _ppsClientViewModel = ppsClientViewModel;
            _ppsClientViewModel.OnEmbeddedCollectionRefreshed += _ppsClientViewModel_OnEmbeddedCollectionRefreshed;
            AddDeviceCmd = new RelayCommand(AddDevice);
            UpdateDeviceCmd = new RelayCommand(UpdateDevice);
            DeleteDeviceCmd = new RelayCommand(DeleteDevice);
        }

        public ObservableCollection<DeviceInfoDataContract> DeviceCollection
        {
            get { return _ppsClientViewModel.DeviceCollection; }
            set
            {
                _ppsClientViewModel.DeviceCollection = value;
                NotifyPropertyChanged(nameof(DeviceCollection));
            }
        }

        public ObservableCollection<PlatformDataContract> PlatformCollection
        {
            get { return _ppsClientViewModel.PlatformCollection; }
            set
            {
                _ppsClientViewModel.PlatformCollection = value;
                NotifyPropertyChanged(nameof(PlatformCollection));
            }
        }

        public DeviceInfoDataContract SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                if (_selectedDevice == value)
                    return;

                _selectedDevice = value;

                if (_selectedDevice == null)
                    return;

                DeviceName = _selectedDevice.DeviceName;
                DeviceSerialNumber = _selectedDevice.DeviceSerialNumber;
                DeviceMacAddress = _selectedDevice.DeviceMacAddress;
                DeviceIpAddress = _selectedDevice.DeviceIpAddress;
                PlatformId = _selectedDevice.DevicePlatformId;

                if (PlatformCollection != null && PlatformCollection.Any())
                    SelectedPlatform =
                        PlatformCollection.FirstOrDefault(x => x.PlatformId == _selectedDevice.DevicePlatformId);

                NotifyPropertyChanged(nameof(SelectedDevice));
            }
        }

        public PlatformDataContract SelectedPlatform
        {
            get { return _selectedPlatform; }
            set
            {
                if (_selectedPlatform == value)
                    return;

                _selectedPlatform = value;

                if (_selectedPlatform == null)
                    return;

                _platformId = _selectedPlatform.PlatformId;
                NotifyPropertyChanged(nameof(SelectedPlatform));
            }
        }

        public string DeviceName
        {
            get { return _deviceName; }
            set
            {
                _deviceName = value;
                NotifyPropertyChanged(nameof(DeviceName));
            }
        }

        public string DeviceSerialNumber
        {
            get { return _deviceSerialNumber; }
            set
            {
                _deviceSerialNumber = value;
                NotifyPropertyChanged(nameof(DeviceSerialNumber));
            }
        }

        public string DeviceMacAddress
        {
            get { return _deviceMacAddress; }
            set
            {
                _deviceMacAddress = value;
                NotifyPropertyChanged(nameof(DeviceMacAddress));
            }
        }

        public string DeviceIpAddress
        {
            get { return _deviceIpAddress; }
            set
            {
                _deviceIpAddress = value;
                NotifyPropertyChanged(nameof(DeviceIpAddress));
            }
        }

        public short PlatformId
        {
            get { return _platformId; }
            set
            {
                if (_platformId == value)
                    return;

                _platformId = value;
                SelectedPlatform = PlatformCollection.FirstOrDefault(x => x.PlatformId == _platformId) ??
                                   PlatformCollection.FirstOrDefault();
                NotifyPropertyChanged(nameof(PlatformId));
            }
        }

        public ICommand AddDeviceCmd { get; private set; }

        public ICommand UpdateDeviceCmd { get; private set; }

        public ICommand DeleteDeviceCmd { get; private set; }

        private void _ppsClientViewModel_OnEmbeddedCollectionRefreshed(string collectionName)
        {
            NotifyPropertyChanged(collectionName);
        }

        private void AddDevice(object param)
        {
            try
            {
                if (string.IsNullOrEmpty(DeviceName.Trim()) || string.IsNullOrEmpty(DeviceSerialNumber.Trim()) ||
                    string.IsNullOrEmpty(DeviceIpAddress.Trim()) || PlatformId < 1)
                    throw new Exception("Не заполнены обязательные поля");

                using (var deviceService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<IDeviceService>())
                {
                    var channel = deviceService.CreateChannel();
                    var result =
                        channel.AddDevice(new DeviceInfoDataContract
                        {
                            DeviceName = DeviceName.Trim(),
                            DeviceSerialNumber = DeviceSerialNumber.Trim(),
                            DeviceMacAddress = DeviceMacAddress.Trim(),
                            DeviceIpAddress = DeviceIpAddress.Trim(),
                            DevicePlatformId = PlatformId
                        });

                    if (!result.BoolRes || !string.IsNullOrEmpty(result.ErrorRes))
                        throw new Exception(result.ErrorRes);

                    _ppsClientViewModel.GetDeviceCollection();
                    _ppsClientViewModel.AddEvent(EventConstants.EVENT_ADD_DEVICE, DeviceSerialNumber);
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage("Не удалось добавить новое устройство - " + ex.Message);
            }
        }

        private void UpdateDevice(object param)
        {
            try
            {
                if (SelectedDevice == null)
                    throw new Exception("Устройство не выбрано");

                if (string.IsNullOrEmpty(DeviceName.Trim()) || string.IsNullOrEmpty(DeviceSerialNumber.Trim()) ||
                    string.IsNullOrEmpty(DeviceIpAddress.Trim()) || PlatformId < 1)
                    throw new Exception("Не заполнены обязательные поля");

                using (var deviceService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<IDeviceService>())
                {
                    var channel = deviceService.CreateChannel();
                    var result =
                        channel.UpdateDevice(SelectedDevice, new DeviceInfoDataContract
                        {
                            DeviceId = SelectedDevice.DeviceId,
                            DeviceName = DeviceName.Trim(),
                            DeviceSerialNumber = DeviceSerialNumber.Trim(),
                            DeviceMacAddress = DeviceMacAddress.Trim(),
                            DeviceIpAddress = DeviceIpAddress.Trim(),
                            DevicePlatformId = PlatformId
                        });

                    if (!result.BoolRes || !string.IsNullOrEmpty(result.ErrorRes))
                        throw new Exception(result.ErrorRes);

                    _ppsClientViewModel.GetDeviceCollection();
                    _ppsClientViewModel.AddEvent(EventConstants.EVENT_UPD_DEVICE, SelectedDevice.DeviceSerialNumber);
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage("Не удалось изменить устройство - " + ex.Message);
            }
        }

        private void DeleteDevice(object param)
        {
            try
            {
                if (SelectedDevice == null)
                    throw new Exception("Устройство не выбрано");

                using (var deviceService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<IDeviceService>())
                {
                    var channel = deviceService.CreateChannel();
                    var result = channel.DeleteDevice(SelectedDevice);

                    if (!result.BoolRes || !string.IsNullOrEmpty(result.ErrorRes))
                        throw new Exception(result.ErrorRes);

                    _ppsClientViewModel.GetDeviceCollection();
                    _ppsClientViewModel.AddEvent(EventConstants.EVENT_DEL_DEVICE, SelectedDevice.DeviceSerialNumber);
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage("Не удалось удалить устройство - " + ex.Message);
            }
        }
    }
}