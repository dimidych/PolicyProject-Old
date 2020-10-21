using System;
using System.Runtime.Serialization;

namespace PolicyProjectManagementService
{
    [DataContract]
    public class PlatformDataContract
    {
        [DataMember(Order = 0)]
        public short PlatformId { get; set; }

        [DataMember(Order = 1)]
        public string PlatformName { get; set; }

        public static tbl_platform FromPlatformDataContractToTblPlatform(PlatformDataContract platformData)
        {
            if (platformData.PlatformId < 1 || string.IsNullOrEmpty(platformData.PlatformName))
                return null;

            return new tbl_platform
            {
                id = platformData.PlatformId,
                platform_name = platformData.PlatformName
            };
        }

        public static PlatformDataContract FromTblPlatformToPlatformDataContract(tbl_platform platformTblData)
        {
            if (platformTblData.id < 1 || string.IsNullOrEmpty(platformTblData.platform_name))
                return null;

            return new PlatformDataContract
            {
                PlatformId = platformTblData.id,
                PlatformName = platformTblData.platform_name
            };
        }

        public static bool Compare(PlatformDataContract obj1, PlatformDataContract obj2)
        {
            if (obj1 == null && obj2 == null)
                return true;

            if (obj1 == null && obj2 != null)
                return false;

            if (obj1 != null && obj2 == null)
                return false;

            return obj1.PlatformId == obj2.PlatformId &&
                   string.Equals(obj1.PlatformName.Trim(), obj2.PlatformName.Trim(),
                       StringComparison.CurrentCultureIgnoreCase);
        }
    }
}