using System;
using System.Runtime.Serialization;

namespace PolicyProjectManagementService
{
    [DataContract]
    public class DeviceInfoDataContract
    {
        [DataMember(Order = 0)]
        public long DeviceId { get; set; }

        [DataMember(Order = 1)]
        public string DeviceName { get; set; }

        [DataMember(Order = 2)]
        public string DeviceSerialNumber { get; set; }

        [DataMember(Order = 3)]
        public string DeviceMacAddress { get; set; }

        [DataMember(Order = 4)]
        public string DeviceIpAddress { get; set; }

        [DataMember(Order = 5)]
        public short DevicePlatformId { get; set; }

        [DataMember(Order = 6)]
        public string DevicePlatformName { get; set; }

        public static tbl_device_info FromDeviceInfoDataContractToTblDeviceInfo(DeviceInfoDataContract deviceData)
        {
            if (deviceData.DeviceId < 1 || string.IsNullOrEmpty(deviceData.DeviceName) ||
                string.IsNullOrEmpty(deviceData.DeviceSerialNumber))
                return null;

            return new tbl_device_info
            {
                id = deviceData.DeviceId,
                device_name = deviceData.DeviceName,
                device_ip_addr = deviceData.DeviceIpAddress,
                device_mac_addr = deviceData.DeviceMacAddress,
                device_platform_id = deviceData.DevicePlatformId,
                device_serial_number = deviceData.DeviceSerialNumber
            };
        }

        public static DeviceInfoDataContract FromTblDeviceInfoToDeviceInfoDataContract(tbl_device_info deviceTblData)
        {
            if (deviceTblData.id < 1 || string.IsNullOrEmpty(deviceTblData.device_name) ||
                string.IsNullOrEmpty(deviceTblData.device_serial_number))
                return null;

            return new DeviceInfoDataContract
            {
                DeviceId = deviceTblData.id,
                DeviceName = deviceTblData.device_name,
                DeviceSerialNumber = deviceTblData.device_serial_number,
                DevicePlatformId = deviceTblData.device_platform_id,
                DeviceIpAddress = deviceTblData.device_ip_addr,
                DeviceMacAddress = deviceTblData.device_mac_addr
            };
        }

        public static bool Compare(DeviceInfoDataContract obj1, DeviceInfoDataContract obj2)
        {
            if (obj1 == null && obj2 == null)
                return true;

            if (obj1 == null && obj2 != null)
                return false;

            if (obj1 != null && obj2 == null)
                return false;

            return obj1.DeviceId == obj2.DeviceId &&
                   string.Equals(obj1.DeviceName.Trim(), obj2.DeviceName.Trim(), StringComparison.CurrentCultureIgnoreCase) &&
                   string.Equals(obj1.DeviceSerialNumber.Trim(), obj2.DeviceSerialNumber.Trim(), StringComparison.CurrentCultureIgnoreCase);
        }
    }
}