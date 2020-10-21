using System.Collections.ObjectModel;
using PolicyProjectManagementService;
using PpsClientChannelProxy;

namespace PolicyProjectManagementClient
{
    public interface IPpsClientViewModel
    {
        event EmbeddedCollectionRefreshed OnEmbeddedCollectionRefreshed;
        event MessageSended OnMessageSended;

        long LoginId { set; }
        long DeviceId { set; }
        string Device { set; }
        string Login { set; }
        IPolicyProjectClientChannelProxy ServiceProxy { get; }
        ObservableCollection<UserDataContract> UserCollection { get; set; }
        ObservableCollection<DeviceInfoDataContract> DeviceCollection { get; set; }
        ObservableCollection<PlatformDataContract> PlatformCollection { get; set; }
        ObservableCollection<LoginDataContract> LoginCollection { get; set; }
        ObservableCollection<GroupDataContract> GroupCollection { get; set; }
        ObservableCollection<EventDataContract> EventCollection { get; set; }
        ObservableCollection<PolicyDataContract> PolicyCollection { get; set; }
        ObservableCollection<EventLogDataContract> EventLogCollection { get; set; }

        void WriteLogMessage(string message);
        void UpdateUserCollection();
        void GetDeviceCollection();
        void GetPlatformCollection();
        void UpdateGroupCollection();
        void UpdateLoginCollection();
        void UpdateEventCollection();
        void UpdatePolicyCollection();
        void UpdateEventLogCollection();
        void AddEvent(int eventId, string message);
    }
}