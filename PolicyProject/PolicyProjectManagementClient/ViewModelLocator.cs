using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using NotifyHelper;
using PpsClientChannelProxy;
using RelayCmd;

namespace PolicyProjectManagementClient
{
    public class ViewModelLocator : Notifier, IDisposable
    {
        private IPolicyProjectClientChannelProxy _serviceProxy;
        private string _loginnedAs;
        private string _logMessage;
        private IPpsClientViewModel _ppsClientVm;
        private bool _disposed = false;

        public ViewModelLocator()
        {
            _ppsClientVm = new PpsClientViewModel(ServiceProxy);
            _ppsClientVm.OnMessageSended += PpsClientVm_OnMessageSended;
            StartVm = new StartViewModel(_ppsClientVm);
            DeviceVm = new DeviceViewModel(_ppsClientVm);
            EventVm = new EventViewModel(_ppsClientVm);
            EventLogVm = new EventLogViewModel(_ppsClientVm);
            GroupVm = new GroupsViewModel(_ppsClientVm);
            LoginVm = new LoginViewModel(_ppsClientVm);
            LoginDevicesVm = new LoginDevicesViewModel(_ppsClientVm);
            PolicyVm = new PolicyViewModel(_ppsClientVm);
            PolicySetVm = new PolicySetViewModel(_ppsClientVm);
            UsersVm = new UsersViewModel(_ppsClientVm);
            ClearLogCmd = new RelayCommand(ClearLog);
            StartVm.OnLoginApplied += StartVm_OnLoginApplied;
        }

        private void PpsClientVm_OnMessageSended(string message)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal,
                (ThreadStart) (() => { LogMessage += message + "\n"; }));
        }

        public long LoginId { get; private set; }

        public string Login { get; private set; }

        public string LoginName { get; private set; }

        public string LoginnedAs
        {
            get { return _loginnedAs; }
            set
            {
                _loginnedAs = value;
                NotifyPropertyChanged(nameof(LoginnedAs));
            }
        }

        public string LogMessage
        {
            get { return _logMessage; }
            set
            {
                _logMessage = value;
                NotifyPropertyChanged(nameof(LogMessage));
            }
        }

        public ICommand ClearLogCmd { get; private set; }

        public StartViewModel StartVm { get; private set; }

        public DeviceViewModel DeviceVm { get; private set; }

        public EventViewModel EventVm { get; private set; }

        public EventLogViewModel EventLogVm { get; private set; }

        public GroupsViewModel GroupVm { get; private set; }

        public LoginViewModel LoginVm { get; private set; }

        public LoginDevicesViewModel LoginDevicesVm { get; private set; }

        public PolicyViewModel PolicyVm { get; private set; }

        public PolicySetViewModel PolicySetVm { get; private set; }

        public UsersViewModel UsersVm { get; private set; }

        protected IPolicyProjectClientChannelProxy ServiceProxy
        {
            get
            {
                if (_serviceProxy != null)
                    return _serviceProxy;

                var baseServiceHostApi = ConfigurationManager.AppSettings["ServiceIp"];
                var serviceAssembly = ConfigurationManager.AppSettings["ServiceAssembly"];
                serviceAssembly = Path.Combine(Environment.CurrentDirectory, serviceAssembly);
                var serviceNameLst = CreateServiceNameList();
                _serviceProxy = new PolicyProjectClientChannelProxy(serviceNameLst, serviceAssembly, baseServiceHostApi);
                return _serviceProxy;
            }
        }

        private static List<string> CreateServiceNameList()
        {
            var serviceNameList = ConfigurationManager.GetSection("ServiceNameList") as NameValueCollection;

            if (serviceNameList == null || serviceNameList.Count == 0)
                return null;

            var serviceNameLst =
                serviceNameList.AllKeys.Select(serviceKey => serviceNameList.GetValues(serviceKey).FirstOrDefault())
                    .ToList();
            return serviceNameLst;
        }

        private void StartVm_OnLoginApplied(LoginEventArgs loginArgs)
        {
            _ppsClientVm.LoginId = loginArgs.LoginId;
            _ppsClientVm.DeviceId = loginArgs.DeviceId;
            _ppsClientVm.Device = loginArgs.Device;
            _ppsClientVm.Login = Login = loginArgs.Login;
            LoginName = loginArgs.LoginName;
            LoginnedAs = string.Concat("Вы вошли как ", LoginName);
        }

        public void ClearLog(object arg)
        {
            LogMessage = string.Empty;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                try
                {
                    _ppsClientVm.AddEvent(EventConstants.EVENT_LOGOUT, Login);
                }
                catch
                {
                }

            _disposed = true;
        }
    }
}