using System.Runtime.Serialization;

namespace PolicyProjectManagementService
{
    [DataContract]
    public class PolicySetDataContract
    {
        [DataMember]
        public long PolicySetId { get; set; }

        [DataMember]
        public int PolicyId { get; set; }

        [DataMember]
        public long? LoginId { get; set; }

        [DataMember]
        public int? GroupId { get; set; }

        [DataMember]
        public bool? Selected { get; set; }

        [DataMember]
        public string PolicyName { get; set; }

        [DataMember]
        public short? PlatformId { get; set; }

        [DataMember]
        public string PolicyInstruction { get; set; }

        [DataMember]
        public string PolicyParam { get; set; }

        public static tbl_policy_set FromPolicySetDataContractToTblPolicySet(PolicySetDataContract policySetData)
        {
            if (policySetData.PolicySetId < 1 || policySetData.PolicyId < 1)
                return null;

            return new tbl_policy_set
            {
                id = policySetData.PolicySetId,
                id_group = policySetData.GroupId,
                id_policy = policySetData.PolicyId,
                id_login = policySetData.LoginId,
                selected = policySetData.Selected,
                policy_param = policySetData.PolicyParam
            };
        }

        public static PolicySetDataContract FromTblPolicySetToPolicySetDataContract(tbl_policy_set policySetTblData)
        {
            if (policySetTblData.id < 1 || policySetTblData.id_policy < 1)
                return null;

            return new PolicySetDataContract
            {
                PolicySetId = policySetTblData.id,
                PolicyId = policySetTblData.id_policy,
                LoginId = policySetTblData.id_login,
                GroupId = policySetTblData.id_group,
                Selected = policySetTblData.selected,
                PolicyParam = policySetTblData.policy_param
            };
        }

        public static bool Compare(PolicySetDataContract obj1, PolicySetDataContract obj2)
        {
            if (obj1 == null && obj2 == null)
                return true;

            if (obj1 == null && obj2 != null)
                return false;

            if (obj1 != null && obj2 == null)
                return false;

            return obj1.PolicySetId == obj2.PolicySetId && obj1.PolicyId == obj2.PolicyId
                   && obj1.LoginId == obj2.LoginId && obj1.GroupId == obj2.GroupId && obj1.PolicyParam == obj2.PolicyParam;
        }
    }
}