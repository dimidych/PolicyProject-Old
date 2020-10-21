using System;
using System.Linq;
using System.Management;
using System.Windows.Input;
using CryptoToolLib;
using NotifyHelper;
using PolicyProjectManagementService;
using RelayCmd;

namespace PolicyProjectManagementClient
{
    public class StartViewModel : Notifier
    {
        public event LoginApplied OnLoginApplied;
        private readonly IPpsClientViewModel _ppsClientViewModel;
        private string _userId;
        private string _statusMessage;
        private bool _loginNotApplied = true;

        public StartViewModel(IPpsClientViewModel ppsClientViewModel)
        {
            _ppsClientViewModel = ppsClientViewModel;
            ApplyLoginCmd = new RelayCommand(ApplyLogin);
        }

        public bool LoginNotApplied
        {
            get { return _loginNotApplied; }
            set
            {
                _loginNotApplied = value;
                NotifyPropertyChanged(nameof(LoginNotApplied));
            }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                _statusMessage = value;
                NotifyPropertyChanged(nameof(StatusMessage));
            }
        }

        public string UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                NotifyPropertyChanged(nameof(UserId));
            }
        }

        public ICommand ApplyLoginCmd { get; private set; }

        private void ApplyLogin(object parameter)
        {
            try
            {
                if (string.IsNullOrEmpty(UserId.Trim()))
                    throw new Exception("Логин не задан");

                var passwordContainer = parameter as IPasswordContainer;

                if (passwordContainer == null || string.Equals(passwordContainer.HashedPassword, Hasher.Hash(null)))
                    throw new Exception("Пароль не задан");

                using (var loginService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<ILoginService>())
                {
                    var channel = loginService.CreateChannel();
                    var selected = channel.GetLogin(new LoginDataContract {Login = UserId});

                    if (!selected.BoolRes)
                        throw new Exception(selected.ErrorRes);

                    if (selected.SomeResult == null || !selected.SomeResult.Any())
                        throw new Exception("Логин не найден");

                    var user = selected.SomeResult.FirstOrDefault();

                    if (!string.Equals(passwordContainer.HashedPassword, user.Password))
                        throw new Exception("Не верный пароль");

                    if (!user.GroupName.ToLower().Contains("администратор"))
                        throw new Exception("Пользователь не администратор");

                    if (string.IsNullOrEmpty(user.Certificate))
                        throw new Exception("Пустой сертификат");

                    SerializeCertificate(user.Certificate);
                    string deviceSerial;
                    var deviceId = GetCurrentMachineId(out deviceSerial); 
                    RaiseOnLoginApplied(user.LoginId, deviceId, deviceSerial, user.Login, user.UserName);
                    _ppsClientViewModel.AddEvent(EventConstants.EVENT_LOGON, user.UserName);
                    LoginNotApplied = false;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                _ppsClientViewModel.AddEvent(EventConstants.EVENT_ERROR, string.Concat("Ошибка входа. ", ex.Message));
            }
        }

        private static void SerializeCertificate(string base64Cert)
        {
            var keyBytes = Convert.FromBase64String(base64Cert);
            var cryptoWorker = new CryptoWorker(CryptoSystemType.RSA, string.Empty, false);
            cryptoWorker.ImportKeyBlob(keyBytes);
        }

        //wmic csproduct get UUID
        private long GetCurrentMachineId(out string guidResult)
        {
            var scope = new ManagementScope("\\\\localhost\\root\\CIMV2", null);
            scope.Connect();
            var query = new ObjectQuery("SELECT UUID FROM Win32_ComputerSystemProduct");

            using (var searcher = new ManagementObjectSearcher(scope, query))
            {
                guidResult = string.Empty;

                foreach (ManagementObject wmiObject in searcher.Get())
                    guidResult = wmiObject["UUID"].ToString();

                using (var deviceService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<IDeviceService>())
                {
                    var channel = deviceService.CreateChannel();
                    var deviceInfo =
                        channel.GetDevice(new DeviceInfoDataContract {DeviceSerialNumber = guidResult});

                    if (!deviceInfo.BoolRes)
                        throw new Exception(deviceInfo.ErrorRes);

                    if (deviceInfo.SomeResult == null || !deviceInfo.SomeResult.Any() ||
                        deviceInfo.SomeResult.FirstOrDefault() == null)
                        throw new Exception("Устройство не найдено");

                    return deviceInfo.SomeResult.FirstOrDefault().DeviceId;
                }
            }
        }

        private void RaiseOnLoginApplied(long loginId, long deviceId, string device, string login, string loginName)
        {
            var dlg = OnLoginApplied;
            dlg?.Invoke(new LoginEventArgs(loginId, deviceId, device, login, loginName));
        }
    }
}