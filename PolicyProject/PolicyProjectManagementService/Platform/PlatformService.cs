using System;
using System.Linq;
using Newtonsoft.Json;
using ServiceRealizationAtribute;

namespace PolicyProjectManagementService
{
    [ServiceRealization(
        InterfaceName = "PolicyProjectManagementService.IPlatformService",
        Name = "PolicyProjectManagementService.PlatformService", OrderToLoad = 0)]
    public class PlatformService : IPlatformService
    {
        public Result<PlatformDataContract[]> GetPlatform(PlatformDataContract platformFilter)
        {
            var result = new Result<PlatformDataContract[]>();

            try
            {
                using (var ctx = new PolicyProjectEntities())
                {
                    if (platformFilter == null)
                        result.SomeResult =
                            ctx.tbl_platform.Select(
                                platformTblData => new PlatformDataContract
                                {
                                    PlatformId = platformTblData.id,
                                    PlatformName = platformTblData.platform_name
                                }).ToArray();
                    else
                        result.SomeResult = ctx.tbl_platform.Where(x => x.id == platformFilter.PlatformId)
                            .Select(platformTblData => new PlatformDataContract
                            {
                                PlatformId = platformTblData.id,
                                PlatformName = platformTblData.platform_name
                            })
                            .ToArray();

                    result.BoolRes = true;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка получения списка платформ. ", ex.Message);
            }

            return result;
        }

        public string GetPlatformRest(PlatformDataContract platformFilter)
        {
            var queryResult = GetPlatform(platformFilter);
            return JsonConvert.SerializeObject(queryResult);
        }
    }
}