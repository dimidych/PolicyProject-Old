using System;
using System.Runtime.Serialization;

namespace PolicyProjectManagementService
{
    [DataContract]
    public class PolicyDataContract
    {
        [DataMember]
        public int PolicyId { get; set; }

        [DataMember]
        public string PolicyName { get; set; }

        [DataMember]
        public short? PlatformId { get; set; }

        [DataMember]
        public string PolicyInstruction { get; set; }

        [DataMember]
        public string PlatformName { get; set; }

        [DataMember]
        public string PolicyDefaultParam { get; set; }

        public static tbl_policy FromPolicyDataContractToTblPolicy(PolicyDataContract policyData)
        {
            if (policyData.PolicyId < 1 || string.IsNullOrEmpty(policyData.PolicyName))
                return null;

            return new tbl_policy
            {
                id = policyData.PolicyId,
                policy_name = policyData.PolicyName,
                platform_id = policyData.PlatformId,
                policy_instruction = policyData.PolicyInstruction,
                policy_default_param = policyData.PolicyDefaultParam
            };
        }

        public static PolicyDataContract FromTblPolicyToPolicyDataContract(tbl_policy policyTblData)
        {
            if (policyTblData.id < 1 || string.IsNullOrEmpty(policyTblData.policy_name))
                return null;

            return new PolicyDataContract
            {
                PolicyId = policyTblData.id,
                PolicyName = policyTblData.policy_name,
                PlatformId = policyTblData.platform_id,
                PolicyInstruction = policyTblData.policy_instruction,
                PolicyDefaultParam = policyTblData.policy_default_param
            };
        }

        public static bool Compare(PolicyDataContract obj1, PolicyDataContract obj2)
        {
            if (obj1 == null && obj2 == null)
                return true;

            if (obj1 == null && obj2 != null)
                return false;

            if (obj1 != null && obj2 == null)
                return false;

            return obj1.PolicyId == obj2.PolicyId &&
                   string.Equals(obj1.PolicyName.Trim(), obj2.PolicyName.Trim(),
                       StringComparison.CurrentCultureIgnoreCase) &&
                   obj1.PlatformId == obj2.PlatformId &&
                   string.Equals(obj1.PolicyInstruction.Trim(), obj2.PolicyInstruction.Trim(),
                       StringComparison.CurrentCultureIgnoreCase);
        }
    }
}