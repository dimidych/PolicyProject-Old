using System.Runtime.Serialization;

namespace PolicyProjectManagementService
{
    [DataContract]
    public class LoginDevicesDataContract
    {
        [DataMember]
        public long LoginDeviceId { get; set; }

        [DataMember]
        public long LoginId { get; set; }

        [DataMember]
        public long DeviceId { get; set; }

        [DataMember]
        public bool? NeedUpdateDevice { get; set; }

        public static tbl_login_devices FromLoginDeviceDataContractToTblLoginDevice(
            LoginDevicesDataContract loginDeviceData)
        {
            if (loginDeviceData.LoginDeviceId < 1 || loginDeviceData.LoginId < 1 || loginDeviceData.DeviceId < 1)
                return null;

            return new tbl_login_devices
            {
                id = loginDeviceData.LoginDeviceId,
                id_login = loginDeviceData.LoginId,
                id_device = loginDeviceData.DeviceId,
                need_update_device = loginDeviceData.NeedUpdateDevice
            };
        }

        public static LoginDevicesDataContract FromTblLoginDevicesToLoginDevicesDataContract(
            tbl_login_devices loginDevicesTblData)
        {
            if (loginDevicesTblData.id < 1 || loginDevicesTblData.id_login < 1 || loginDevicesTblData.id_device < 1)
                return null;

            return new LoginDevicesDataContract
            {
                LoginDeviceId = loginDevicesTblData.id,
                DeviceId = loginDevicesTblData.id_device,
                LoginId = loginDevicesTblData.id_login,
                NeedUpdateDevice = loginDevicesTblData.need_update_device
            };
        }

        public static bool Compare(LoginDevicesDataContract obj1, LoginDevicesDataContract obj2)
        {
            if (obj1 == null && obj2 == null)
                return true;

            if (obj1 == null && obj2 != null)
                return false;

            if (obj1 != null && obj2 == null)
                return false;

            return obj1.LoginDeviceId == obj2.LoginDeviceId && obj1.DeviceId == obj2.DeviceId
                   && obj1.LoginId == obj2.LoginId;
        }
    }
}