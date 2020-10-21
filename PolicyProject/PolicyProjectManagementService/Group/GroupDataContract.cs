using System;
using System.Runtime.Serialization;

namespace PolicyProjectManagementService
{
    [DataContract]
    public class GroupDataContract
    {
        [DataMember(Order = 0)]
        public int GroupId { get; set; }

        [DataMember(Order = 1)]
        public string GroupName { get; set; }

        public static tbl_group FromGroupDataContractToTblGroup(GroupDataContract groupData)
        {
            if (groupData.GroupId < 1 || string.IsNullOrEmpty(groupData.GroupName))
                return null;

            return new tbl_group
            {
                id = groupData.GroupId,
                group_name = groupData.GroupName
            };
        }

        public static GroupDataContract FromTblGroupToGroupDataContract(tbl_group groupTblData)
        {
            if (groupTblData.id < 1 || string.IsNullOrEmpty(groupTblData.group_name))
                return null;

            return new GroupDataContract {GroupId = groupTblData.id, GroupName = groupTblData.group_name};
        }

        public static bool Compare(GroupDataContract obj1, GroupDataContract obj2)
        {
            if (obj1 == null && obj2 == null)
                return true;

            if (obj1 == null && obj2 != null)
                return false;

            if (obj1 != null && obj2 == null)
                return false;

            return obj1.GroupId == obj2.GroupId &&
                   string.Equals(obj1.GroupName.Trim(), obj2.GroupName.Trim(), StringComparison.CurrentCultureIgnoreCase);
        }
    }
}