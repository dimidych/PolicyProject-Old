using System;
using System.Collections.ObjectModel;
using NotifyHelper;
using PolicyProjectManagementService;
using PpsClientChannelProxy;

namespace PolicyProjectManagementClient
{
    public class PpsClientViewModel : Notifier, IPpsClientViewModel
    {
        public event EmbeddedCollectionRefreshed OnEmbeddedCollectionRefreshed;
        public event MessageSended OnMessageSended;

        public PpsClientViewModel(IPolicyProjectClientChannelProxy serviceProxy)
        {
            ServiceProxy = serviceProxy;
            UpdateUserCollection();
            GetPlatformCollection();
            GetDeviceCollection();
            UpdateLoginCollection();
            UpdateGroupCollection();
            UpdateEventCollection();
            UpdatePolicyCollection();
            UpdateEventLogCollection();
        }

        public IPolicyProjectClientChannelProxy ServiceProxy { get; }

        public ObservableCollection<UserDataContract> UserCollection { get; set; }

        public ObservableCollection<DeviceInfoDataContract> DeviceCollection { get; set; }

        public ObservableCollection<PlatformDataContract> PlatformCollection { get; set; }

        public ObservableCollection<LoginDataContract> LoginCollection { get; set; }

        public ObservableCollection<GroupDataContract> GroupCollection { get; set; }

        public ObservableCollection<EventDataContract> EventCollection { get; set; }

        public ObservableCollection<PolicyDataContract> PolicyCollection { get; set; }

        public ObservableCollection<EventLogDataContract> EventLogCollection { get; set; }

        public long LoginId { private get; set; }

        public long DeviceId { private get; set; }

        public string Device { private get; set; }

        public string Login { private get; set; }

        public void UpdateEventLogCollection()
        {
            try
            {
                using (var eventLogService = ServiceProxy.GetPpsChannelFactory<IEventLogService>())
                {
                    var channel = eventLogService.CreateChannel();
                    var result = channel.GetEventLog(null);

                    if (result.BoolRes && result.SomeResult != null)
                    {
                        EventLogCollection = new ObservableCollection<EventLogDataContract>(result.SomeResult);
                        RaiseOnEmbeddedCollectionRefreshed(nameof(EventLogCollection));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLogMessage("Не удалось прочитать лог событий - " + ex.Message);
            }
        }

        public void UpdateUserCollection()
        {
            try
            {
                using (var userService = ServiceProxy.GetPpsChannelFactory<IUserService>())
                {
                    var channel = userService.CreateChannel();
                    var result = channel.GetUser(null);

                    if (result.BoolRes && result.SomeResult != null)
                    {
                        UserCollection = new ObservableCollection<UserDataContract>(result.SomeResult);
                        RaiseOnEmbeddedCollectionRefreshed(nameof(UserCollection));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLogMessage("Не удалось получить список пользователей - " + ex.Message);
            }
        }

        public void GetDeviceCollection()
        {
            try
            {
                using (var deviceService = ServiceProxy.GetPpsChannelFactory<IDeviceService>())
                {
                    var channel = deviceService.CreateChannel();
                    var result = channel.GetDevice(null);

                    if (result.BoolRes && result.SomeResult != null)
                    {
                        DeviceCollection = new ObservableCollection<DeviceInfoDataContract>(result.SomeResult);
                        RaiseOnEmbeddedCollectionRefreshed(nameof(DeviceCollection));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLogMessage("Не удалось получить список устройств - " + ex.Message);
            }
        }

        public void GetPlatformCollection()
        {
            try
            {
                using (var platformService = ServiceProxy.GetPpsChannelFactory<IPlatformService>())
                {
                    var channel = platformService.CreateChannel();
                    var result = channel.GetPlatform(null);

                    if (result.BoolRes && result.SomeResult != null)
                    {
                        PlatformCollection = new ObservableCollection<PlatformDataContract>(result.SomeResult);
                        RaiseOnEmbeddedCollectionRefreshed(nameof(PlatformCollection));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLogMessage("Не удалось получить список платформ - " + ex.Message);
            }
        }

        public void UpdateLoginCollection()
        {
            try
            {
                using (var loginService = ServiceProxy.GetPpsChannelFactory<ILoginService>())
                {
                    var channel = loginService.CreateChannel();
                    var result = channel.GetLogin(null);

                    if (result.BoolRes && result.SomeResult != null)
                    {
                        LoginCollection = new ObservableCollection<LoginDataContract>(result.SomeResult);
                        RaiseOnEmbeddedCollectionRefreshed(nameof(LoginCollection));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLogMessage(string.Concat("Не удалось получить список логинов - ", ex.Message));
            }
        }

        public void UpdateGroupCollection()
        {
            try
            {
                using (var groupService = ServiceProxy.GetPpsChannelFactory<IGroupService>())
                {
                    var channel = groupService.CreateChannel();
                    var result = channel.GetGroup(null);

                    if (result.BoolRes && result.SomeResult != null)
                    {
                        GroupCollection = new ObservableCollection<GroupDataContract>(result.SomeResult);
                        RaiseOnEmbeddedCollectionRefreshed(nameof(GroupCollection));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLogMessage(string.Concat("Не удалось получить список групп - ", ex.Message));
            }
        }

        public void UpdateEventCollection()
        {
            try
            {
                using (var eventService = ServiceProxy.GetPpsChannelFactory<IEventService>())
                {
                    var channel = eventService.CreateChannel();
                    var result = channel.GetEvent(null);

                    if (result.BoolRes && result.SomeResult != null)
                    {
                        EventCollection = new ObservableCollection<EventDataContract>(result.SomeResult);
                        RaiseOnEmbeddedCollectionRefreshed(nameof(EventCollection));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLogMessage("Не удалось получить список cобытий - " + ex.Message);
            }
        }

        public void UpdatePolicyCollection()
        {
            try
            {
                using (var policyService = ServiceProxy.GetPpsChannelFactory<IPolicyService>())
                {
                    var channel = policyService.CreateChannel();
                    var result = channel.GetPolicy(null);

                    if (result.BoolRes && result.SomeResult != null)
                    {
                        PolicyCollection = new ObservableCollection<PolicyDataContract>(result.SomeResult);
                        RaiseOnEmbeddedCollectionRefreshed(nameof(PolicyCollection));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLogMessage("Не удалось получить список политик - " + ex.Message);
            }
        }

        private void RaiseOnEmbeddedCollectionRefreshed(string collectionName)
        {
            var dlg = OnEmbeddedCollectionRefreshed;
            dlg?.Invoke(collectionName);
        }

        public void WriteLogMessage(string message)
        {
            AddEvent(EventConstants.EVENT_ERROR, message);
            var dlg = OnMessageSended;
            dlg?.Invoke(message);
        }

        public void AddEvent(int eventId, string message)
        {
            try
            {
                using (var logService = ServiceProxy.GetPpsChannelFactory<IEventLogService>())
                {
                    var channel = logService.CreateChannel();
                    var result = channel.AddEventLogExplicit(Device, Login, eventId, -1, message);

                    if (!result.BoolRes)
                        throw new Exception(result.ErrorRes);
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                UpdateEventLogCollection();
            }
        }
    }
}