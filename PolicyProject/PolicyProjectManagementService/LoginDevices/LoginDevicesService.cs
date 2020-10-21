using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Newtonsoft.Json;
using ServiceRealizationAtribute;

namespace PolicyProjectManagementService
{
    [ServiceRealization(
        InterfaceName = "PolicyProjectManagementService.ILoginDevicesService",
        Name = "PolicyProjectManagementService.LoginDevicesService", OrderToLoad = 0)]
    public class LoginDevicesService : ILoginDevicesService
    {
        public Result<LoginDevicesDataContract[]> GetLoginDevices(LoginDevicesDataContract loginDevicesFilter)
        {
            var result = new Result<LoginDevicesDataContract[]>();

            try
            {
                using (var ctx = new PolicyProjectEntities())
                {
                    if (loginDevicesFilter == null)
                        result.SomeResult =
                            ctx.tbl_login_devices.Select(
                                    loginDevicesTblData => new LoginDevicesDataContract
                                    {
                                        LoginDeviceId = loginDevicesTblData.id,
                                        DeviceId = loginDevicesTblData.id_device,
                                        LoginId = loginDevicesTblData.id_login,
                                        NeedUpdateDevice = loginDevicesTblData.need_update_device
                                    })
                                .ToArray();
                    else
                        result.SomeResult =
                            ctx.tbl_login_devices.Where(
                                    x =>
                                        (loginDevicesFilter.LoginDeviceId > 0 &&
                                         x.id == loginDevicesFilter.LoginDeviceId)
                                        || (loginDevicesFilter.LoginId > 0 && x.id_login == loginDevicesFilter.LoginId)
                                        ||
                                        (loginDevicesFilter.DeviceId > 0 && x.id_device == loginDevicesFilter.DeviceId))
                                .Select(
                                    loginDevicesTblData => new LoginDevicesDataContract
                                    {
                                        LoginDeviceId = loginDevicesTblData.id,
                                        DeviceId = loginDevicesTblData.id_device,
                                        LoginId = loginDevicesTblData.id_login,
                                        NeedUpdateDevice = loginDevicesTblData.need_update_device
                                    }).ToArray();

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

        public string GetLoginDevicesRest(LoginDevicesDataContract loginDevicesFilter)
        {
            var queryResult = GetLoginDevices(loginDevicesFilter);
            return JsonConvert.SerializeObject(queryResult);
        }

        public Result<object> UpdateLoginDevices(long loginId, LoginDevicesDataContract[] loginDevicesList)
        {
            var result = new Result<object>();

            using (var transaction = new TransactionScope())
            {
                try
                {
                    using (var ctx = new PolicyProjectEntities())
                    {
                        var deletedList = new List<tbl_login_devices>();

                        foreach (var loginDevice in ctx.tbl_login_devices.Where(x => x.id_login == loginId))
                            if (loginDevicesList.All(x => x.DeviceId != loginDevice.id_device))
                                deletedList.Add(loginDevice);

                        foreach (var forDeletion in deletedList)
                            ctx.tbl_login_devices.Remove(forDeletion);

                        long newLoginDeviceId = 0;

                        foreach (var selectedDevice in loginDevicesList)
                        {
                            if (
                                ctx.tbl_login_devices.Any(
                                    x => x.id_device == selectedDevice.DeviceId && x.id_login == selectedDevice.LoginId))
                                continue;

                            if (newLoginDeviceId == 0)
                                newLoginDeviceId = ctx.tbl_login_devices.Any()
                                    ? ctx.tbl_login_devices.Max(x => x.id) + 1
                                    : 1;
                            else
                                newLoginDeviceId++;

                            ctx.tbl_login_devices.Add(new tbl_login_devices
                            {
                                id = newLoginDeviceId,
                                id_device = selectedDevice.DeviceId,
                                id_login = selectedDevice.LoginId,
                                need_update_device = selectedDevice.NeedUpdateDevice
                            });
                        }

                        result.BoolRes = ctx.SaveChanges() > 0;

                        if (result.BoolRes)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    transaction.Dispose();
                    result.BoolRes = false;
                    result.ErrorRes = string.Concat("Ошибка сохранения списка устройств логина. ", ex.Message);
                }
            }

            return result;
        }

        public Result<Dictionary<long, long[]>> GetLoginIdAndDeviceIdDct()
        {
            var result = new Result<Dictionary<long, long[]>>();

            try
            {
                using (var ctx = new PolicyProjectEntities())
                {
                    var devices = from dev in ctx.tbl_login_devices
                        group dev by dev.id_login
                        into selectedDevice
                        select new {LoginId = selectedDevice.Key, DevIds = selectedDevice.Select(x => x.id_device)};
                    var resultDct = new Dictionary<long, long[]>();

                    foreach (var devs in devices)
                        resultDct.Add(devs.LoginId, devs.DevIds.ToArray());

                    result.SomeResult = resultDct;
                    result.BoolRes = true;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка удаления наборa политик. ", ex.Message);
            }

            return result;
        }

        public string UpdateLoginDevicesRest(LoginDevicesDataContract loginDevice)
        {
            var result = new Result<object>();

            try
            {
                using (var ctx = new PolicyProjectEntities())
                {
                    var existed = ctx.tbl_login_devices.FirstOrDefault(
                        x => x.id_device == loginDevice.DeviceId && x.id_login == loginDevice.LoginId);

                    if (existed == null)
                        throw new Exception(string.Concat("Не найдено устройство с ИД ", loginDevice.DeviceId,
                            ", принадлежащий логину с ИД ", loginDevice.LoginId));

                    existed.need_update_device = loginDevice.NeedUpdateDevice; 
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка обновления статуса устройства. ", ex.Message);
            }

            return JsonConvert.SerializeObject(result);
        }

        public Result<object> SetDevicesForUpdate(List<long> loginIdList)
        {
            var result = new Result<object>();

            try
            {
                if (loginIdList == null || !loginIdList.Any())
                    throw new Exception("Пустой список устройств для обновления");

                using (var ctx = new PolicyProjectEntities())
                {
                    foreach (var loginDevice in ctx.tbl_login_devices.Where(x => loginIdList.Contains(x.id_login)))
                        loginDevice.need_update_device = true;

                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка сохранения списка устройств для обновления. ", ex.Message);
            }

            return result;
        }
    }
}
