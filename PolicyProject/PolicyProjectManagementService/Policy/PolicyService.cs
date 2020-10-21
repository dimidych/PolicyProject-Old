using System;
using System.Linq;
using Newtonsoft.Json;
using ServiceRealizationAtribute;

namespace PolicyProjectManagementService
{
    [ServiceRealization(
        InterfaceName = "PolicyProjectManagementService.IPolicyService",
        Name = "PolicyProjectManagementService.PolicyService", OrderToLoad = 0)]
    public class PolicyService : IPolicyService
    {
        public Result<PolicyDataContract[]> GetPolicy(PolicyDataContract policyFilter)
        {
            var result = new Result<PolicyDataContract[]>();

            try
            {
                using (var ctx = new PolicyProjectEntities())
                {
                    if (policyFilter == null)
                        result.SomeResult =
                            ctx.tbl_policy.Select(
                                policyTblData => new PolicyDataContract
                                {
                                    PolicyId = policyTblData.id,
                                    PolicyName = policyTblData.policy_name,
                                    PlatformId = policyTblData.platform_id,
                                    PolicyInstruction = policyTblData.policy_instruction,
                                    PolicyDefaultParam = policyTblData.policy_default_param,
                                    PlatformName =
                                        ctx.tbl_platform.FirstOrDefault(
                                            platform => platform.id == policyTblData.platform_id).platform_name
                                }).ToArray();
                    else
                        result.SomeResult = ctx.tbl_policy.Where(x => x.id == policyFilter.PolicyId)
                            .Select(policyTblData => new PolicyDataContract
                            {
                                PolicyId = policyTblData.id,
                                PolicyName = policyTblData.policy_name,
                                PlatformId = policyTblData.platform_id,
                                PolicyInstruction = policyTblData.policy_instruction,
                                PolicyDefaultParam = policyTblData.policy_default_param,
                                PlatformName =
                                    ctx.tbl_platform.FirstOrDefault(platform => platform.id == policyTblData.platform_id)
                                        .platform_name
                            })
                            .ToArray();

                    result.BoolRes = true;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка получения списка политик. ", ex.Message);
            }

            return result;
        }

        public string GetPolicyRest(PolicyDataContract policyFilter)
        {
            var queryResult = GetPolicy(policyFilter);
            return JsonConvert.SerializeObject(queryResult);
        }

        public Result<object> AddPolicy(PolicyDataContract policy)
        {
            var result = new Result<object>();

            try
            {
                if (policy == null)
                    throw new Exception("Новая политика не задана");

                using (var ctx = new PolicyProjectEntities())
                {
                    var selected =
                        ctx.tbl_policy.FirstOrDefault(
                            x =>
                                x.platform_id == policy.PlatformId &&
                                x.policy_name.Trim()
                                    .Equals(policy.PolicyName.Trim(), StringComparison.InvariantCultureIgnoreCase));

                    if (selected != null)
                        throw new Exception("Такая политика уже существует");

                    policy.PolicyId = ctx.tbl_policy.Any() ? ctx.tbl_policy.Max(x => x.id) + 1 : 1;
                    var newPolicy = PolicyDataContract.FromPolicyDataContractToTblPolicy(policy);
                    ctx.tbl_policy.Add(newPolicy);
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка добавления политики. ", ex.Message);
            }

            return result;
        }

        public Result<object> UpdatePolicy(PolicyDataContract oldPolicy, PolicyDataContract newPolicy)
        {
            var result = new Result<object>();

            try
            {
                if (oldPolicy == null)
                    throw new Exception("Текущая политика не задана");

                if (newPolicy == null)
                    throw new Exception("Новая политика не задана");

                using (var ctx = new PolicyProjectEntities())
                {
                    var selected = ctx.tbl_policy.FirstOrDefault(x => x.id == oldPolicy.PolicyId);

                    if (selected == null)
                        throw new Exception("Текущая политика не найдена");

                    selected.policy_name = newPolicy.PolicyName;
                    selected.platform_id = newPolicy.PlatformId;
                    selected.policy_instruction = newPolicy.PolicyInstruction;
                    selected.policy_default_param = newPolicy.PolicyDefaultParam;
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка изменения политики. ", ex.Message);
            }

            return result;
        }

        public Result<object> DeletePolicy(PolicyDataContract policy)
        {
            var result = new Result<object>();

            try
            {
                if (policy == null)
                    throw new Exception("Политика не задана");

                using (var ctx = new PolicyProjectEntities())
                {
                    var delPolicy = ctx.tbl_policy.FirstOrDefault(x => x.id == policy.PolicyId);
                    ctx.tbl_policy.Remove(delPolicy);
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка удаления политики. ", ex.Message);
            }

            return result;
        }
    }
}