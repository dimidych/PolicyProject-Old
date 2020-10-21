using System;
using System.Linq;
using Newtonsoft.Json;
using ServiceRealizationAtribute;

namespace PolicyProjectManagementService
{
    [ServiceRealization(
        InterfaceName = "PolicyProjectManagementService.IDeviceService",
        Name = "PolicyProjectManagementService.DeviceService", OrderToLoad = 0)]
    public class DeviceService : IDeviceService
    {
        public Result<DeviceInfoDataContract[]> GetDevice(DeviceInfoDataContract deviceFilter)
        {
            var result = new Result<DeviceInfoDataContract[]>();

            try
            {
                using (var ctx = new PolicyProjectEntities())
                {
                    if (deviceFilter == null)
                        result.SomeResult =
                            ctx.tbl_device_info.Select(
                                deviceTblData => new DeviceInfoDataContract
                                {
                                    DeviceId = deviceTblData.id,
                                    DeviceName = deviceTblData.device_name,
                                    DeviceSerialNumber = deviceTblData.device_serial_number,
                                    DevicePlatformId = deviceTblData.device_platform_id,
                                    DeviceIpAddress = deviceTblData.device_ip_addr,
                                    DeviceMacAddress = deviceTblData.device_mac_addr,
                                    DevicePlatformName =
                                        ctx.tbl_platform.FirstOrDefault(x => x.id == deviceTblData.device_platform_id)
                                            .platform_name
                                }).ToArray();
                    else
                        result.SomeResult = ctx.tbl_device_info.Where(x => x.id == deviceFilter.DeviceId
                                                                           ||
                                                                           x.device_serial_number.Equals(
                                                                               deviceFilter.DeviceSerialNumber,
                                                                               StringComparison
                                                                                   .InvariantCultureIgnoreCase))
                            .Select(deviceTblData => new DeviceInfoDataContract
                            {
                                DeviceId = deviceTblData.id,
                                DeviceName = deviceTblData.device_name,
                                DeviceSerialNumber = deviceTblData.device_serial_number,
                                DevicePlatformId = deviceTblData.device_platform_id,
                                DeviceIpAddress = deviceTblData.device_ip_addr,
                                DeviceMacAddress = deviceTblData.device_mac_addr,
                                DevicePlatformName =
                                    ctx.tbl_platform.FirstOrDefault(x => x.id == deviceTblData.device_platform_id)
                                        .platform_name
                            })
                            .ToArray();

                    result.BoolRes = true;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка получения списка устройств. ", ex.Message);
            }

            return result;
        }

        public string GetDeviceRest(DeviceInfoDataContract deviceFilter)
        {
            var queryResult = GetDevice(deviceFilter);
            return JsonConvert.SerializeObject(queryResult);
        }

        public Result<object> AddDevice(DeviceInfoDataContract device)
        {
            var result = new Result<object>();

            try
            {
                if (device == null)
                    throw new Exception("Новое устройство не задано");

                using (var ctx = new PolicyProjectEntities())
                {
                    device.DeviceId = ctx.tbl_device_info.Any() ? ctx.tbl_device_info.Max(x => x.id) + 1 : 1;
                    var newDevice = DeviceInfoDataContract.FromDeviceInfoDataContractToTblDeviceInfo(device);
                    ctx.tbl_device_info.Add(newDevice);
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка добавления устройствa. ", ex.Message);
            }

            return result;
        }

        public Result<object> UpdateDevice(DeviceInfoDataContract oldDevice, DeviceInfoDataContract newDevice)
        {
            var result = new Result<object>();

            try
            {
                if (oldDevice == null)
                    throw new Exception("Текущее устройство не задано");

                if (newDevice == null)
                    throw new Exception("Новое устройство не задано");

                using (var ctx = new PolicyProjectEntities())
                {
                    var selected = ctx.tbl_device_info.FirstOrDefault(x => oldDevice.DeviceId == x.id);

                    if (selected == null)
                        throw new Exception("Текущее устройство не найдено");

                    selected.device_name = newDevice.DeviceName;
                    selected.device_ip_addr = newDevice.DeviceIpAddress;
                    selected.device_serial_number = newDevice.DeviceSerialNumber;
                    selected.device_mac_addr = newDevice.DeviceMacAddress;
                    selected.device_platform_id = newDevice.DevicePlatformId;
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка изменения устройствa. ", ex.Message);
            }

            return result;
        }

        public Result<object> DeleteDevice(DeviceInfoDataContract device)
        {
            var result = new Result<object>();

            try
            {
                if (device == null)
                    throw new Exception("Yстройство не задано");

                using (var ctx = new PolicyProjectEntities())
                {
                    var delDevice = ctx.tbl_device_info.FirstOrDefault(x => device.DeviceId == x.id);
                    ctx.tbl_device_info.Remove(delDevice);
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка удаления устройствa. ", ex.Message);
            }

            return result;
        }
    }
}