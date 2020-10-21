using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using NotifyHelper;
using PpcServicePool;
using RelayCmd;

namespace PolicyProjectServiceHost
{
    public class PpsHostViewModel : Notifier
    {
        private IPolicyProjectServicePool _servicePool;
        private string _logMessage;
        private List<string> _serviceNameLst;
        private string _status;
        private readonly string _serviceAssembly;
        private readonly string _baseServiceHostIp;
        private bool _canStart;
        private bool _canStop;

        public PpsHostViewModel()
        {
            try
            {
                CanStart = true;
                CanStop = false;
               _baseServiceHostIp = ConfigurationManager.AppSettings["ServiceIp"];
                WriteLogMessage("IP хоста служб : " + _baseServiceHostIp);
                _serviceAssembly = ConfigurationManager.AppSettings["ServiceAssembly"];
                _serviceAssembly = Path.Combine(Environment.CurrentDirectory, _serviceAssembly);
                WriteLogMessage("Путь к файлу сборки сервисов : " + _serviceAssembly);
                ClearLogCmd = new RelayCommand(ClearLog);
                StartPpsHostCmd = new RelayCommand(StartService);
                StopPpsHostCmd = new RelayCommand(StopService);
                CheckServiceStatusCmd = new RelayCommand(CheckServiceStatus);
                CreateServiceNameList();
            }
            catch (Exception ex)
            {
                WriteLogMessage(ex.Message);
            }
        }

        public ICommand ClearLogCmd { get; private set; }

        public ICommand StartPpsHostCmd { get; private set; }

        public ICommand StopPpsHostCmd { get; private set; }

        public ICommand CheckServiceStatusCmd { get; private set; }

        public bool CanStart
        {
            get { return _canStart;}
            set
            {
                _canStart = value;
                NotifyPropertyChanged(nameof(CanStart));
            }
        }

        public bool CanStop
        {
            get { return _canStop; }
            set
            {
                _canStop = value;
                NotifyPropertyChanged(nameof(CanStop));
            }
        }

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                NotifyPropertyChanged("Status");
            }
        }

        public string LogMessage
        {
            get { return _logMessage; }
            set
            {
                _logMessage = value;
                NotifyPropertyChanged("LogMessage");
            }
        }

        private void CreateServiceNameList()
        {
            _serviceNameLst = new List<string>();
            var serviceNameList = ConfigurationManager.GetSection("ServiceNameList") as NameValueCollection;

            if (serviceNameList == null || serviceNameList.Count == 0)
                return;

            foreach (
                var serviceName in
                    serviceNameList.AllKeys.Select(serviceKey => serviceNameList.GetValues(serviceKey).FirstOrDefault())
                )
            {
                _serviceNameLst.Add(serviceName);
                WriteLogMessage("Сервис " + serviceName + " добавлен в очередь на запуск");
            }
        }

        public void StartService(object arg)
        {
            try
            {
                _servicePool = new PolicyProjectServicePool(_serviceNameLst, _serviceAssembly, _baseServiceHostIp);

                foreach (var service in _servicePool.ServicesHosts)
                    if (service.State != CommunicationState.Opened || service.State != CommunicationState.Opening)
                    {
                        var task = new Task(() =>
                        {
                            service.Open();
                            WriteLogMessage("Сервис " + service.Description.Name + " запущен");
                        });
                        task.Start();
                    }

                Status = "Состояние хоста : все сервисы запущены";
            }
            catch (Exception ex)
            {
                WriteLogMessage(ex.Message);
            }
            finally
            {
                CanStart = false;
                CanStop = true;
            }
        }

        public void StopService(object arg)
        {
            try
            {
                if (_servicePool?.ServicesHosts == null || !_servicePool.ServicesHosts.Any())
                    throw new Exception("Пул сервисов не существует");

                foreach (var service in _servicePool.ServicesHosts)
                    if (service.State != CommunicationState.Closed || service.State != CommunicationState.Closing)
                    {
                        var task = new Task(() =>
                        {
                            service.Close();
                            WriteLogMessage("Сервис " + service.Description.Name + " остановлен");
                        });
                        task.Start();
                    }

                Status = "Состояние хоста : все сервисы остановлены";
            }
            catch (Exception ex)
            {
                WriteLogMessage(ex.Message);
            }
            finally
            {
                CanStart = true;
                CanStop = false;
            }
        }

        public void CheckServiceStatus(object arg)
        {
            try
            {
                foreach (var service in _servicePool.ServicesHosts)
                    WriteLogMessage("Состояние сервиса " + service.Description.Name + " : " + service.State);
            }
            catch (Exception ex)
            {
                WriteLogMessage(ex.Message);
            }
        }

        private void WriteLogMessage(string message)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal,
                (ThreadStart) (() => { LogMessage += message + "\n" + "**********************************" + "\n"; }));
        }

        public void ClearLog(object arg)
        {
            LogMessage = string.Empty;
        }
    }
}