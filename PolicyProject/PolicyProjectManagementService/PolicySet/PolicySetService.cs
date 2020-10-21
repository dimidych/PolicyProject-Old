using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Newtonsoft.Json;
using ServiceRealizationAtribute;

namespace PolicyProjectManagementService
{
    [ServiceRealization(
        InterfaceName = "PolicyProjectManagementService.IPolicySetService",
        Name = "PolicyProjectManagementService.PolicySetService", OrderToLoad = 0)]
    public class PolicySetService : IPolicySetService
    {
        public Result<PolicySetDataContract[]> GetPolicySet(PolicySetDataContract policySetFilter)
        {
            var result = new Result<PolicySetDataContract[]>();

            try
            {
                using (var ctx = new PolicyProjectEntities())
                {
                    if (policySetFilter == null)
                        result.SomeResult =
                            ctx.tbl_policy_set.Select(
                                policySetTblData => new PolicySetDataContract
                                {
                                    PolicySetId = policySetTblData.id,
                                    PolicyId = policySetTblData.id_policy,
                                    LoginId = policySetTblData.id_login,
                                    GroupId = policySetTblData.id_group,
                                    Selected = policySetTblData.selected,
                                    PolicyParam = policySetTblData.policy_param
                                })
                                .ToArray();
                    else
                        result.SomeResult =
                            ctx.tbl_policy_set.Where(
                                x => (policySetFilter.PolicyId > 0 && x.id_policy == policySetFilter.PolicyId)
                                     || (policySetFilter.LoginId > 0 && x.id_login == policySetFilter.LoginId)
                                     || (policySetFilter.GroupId > 0 && x.id_group == policySetFilter.GroupId))
                                .Select(
                                    policySetTblData => new PolicySetDataContract
                                    {
                                        PolicySetId = policySetTblData.id,
                                        PolicyId = policySetTblData.id_policy,
                                        LoginId = policySetTblData.id_login,
                                        GroupId = policySetTblData.id_group,
                                        Selected = policySetTblData.selected,
                                        PolicyParam = policySetTblData.policy_param
                                    }).ToArray();

                    result.BoolRes = true;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка получения набора политик. ", ex.Message);
            }

            return result;
        }

        public string GetPolicySetRest(PolicySetDataContract policySetFilter)
        {
            var queryResult = GetPolicySet(policySetFilter);
            return JsonConvert.SerializeObject(queryResult);
        }

        public string GetPolicySetForLoginRest(PolicySetDataContract policySetFilter)
        {
            var queryResult = GetPolicySetForLogin(policySetFilter.LoginId.Value);

            try
            {
                var policyService = new PolicyService();

                foreach (var policySet in queryResult.SomeResult)
                {
                    var policyResult = policyService.GetPolicy(new PolicyDataContract {PolicyId = policySet.PolicyId});

                    if (!policyResult.BoolRes || policyResult.SomeResult == null || !policyResult.SomeResult.Any())
                        continue;

                    var policyInfo = policyResult.SomeResult[0];
                    policySet.PolicyName = policyInfo.PolicyName;
                    policySet.PlatformId = policyInfo.PlatformId;
                    policySet.PolicyInstruction = policyInfo.PolicyInstruction;
                    policySet.PolicyParam = policySet.PolicyParam ?? policyInfo.PolicyDefaultParam;
                }
            }
            catch (Exception ex)
            {
                queryResult.ErrorRes = string.Concat("Не удалось получить дополнительную информацию о политике. ",
                    ex.Message);
            }

            return JsonConvert.SerializeObject(queryResult);
        }

        public Result<PolicySetDataContract[]> GetPolicySetForGroup(int groupId)
        {
            var result = new Result<PolicySetDataContract[]>();

            try
            {
                using (var ctx = new PolicyProjectEntities())
                {

                    result.SomeResult =
                        ctx.tbl_policy_set.Where(x => x.id_group == groupId)
                            .Select(
                                policySetTblData => new PolicySetDataContract
                                {
                                    PolicySetId = policySetTblData.id,
                                    PolicyId = policySetTblData.id_policy,
                                    PolicyParam = policySetTblData.policy_param
                                }).ToArray();

                    result.BoolRes = true;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка получения набора политик. ", ex.Message);
            }

            return result;
        }

        public Result<PolicySetDataContract[]> GetPolicySetForLogin(long loginId)
        {
            var result = new Result<PolicySetDataContract[]>();

            try
            {
                using (var ctx = new PolicyProjectEntities())
                {
                    var loginInfo = ctx.tbl_login.FirstOrDefault(x => x.id == loginId);

                    if (loginInfo == null)
                        throw new Exception("Логин не найден");

                    var groupPolicySet = GetPolicySetForGroup(loginInfo.id_group).SomeResult;
                    var loginPolicySet =
                        ctx.tbl_policy_set.Where(x => x.id_login == loginId)
                            .Select(
                                policySetTblData => new PolicySetDataContract
                                {
                                    LoginId = loginId,
                                    PolicySetId = policySetTblData.id,
                                    PolicyId = policySetTblData.id_policy,
                                    Selected = policySetTblData.selected,
                                    PolicyParam = policySetTblData.policy_param
                                }).ToList();

                    if (groupPolicySet == null || !groupPolicySet.Any())
                    {
                        result.BoolRes = true;
                        result.SomeResult = loginPolicySet.Where(x => x.Selected.HasValue && x.Selected.Value).ToArray();
                        return result;
                    }

                    var resultLst = groupPolicySet.ToList();

                    foreach (var res in resultLst)
                    {
                        res.GroupId = null;
                        res.LoginId = loginId;
                    }

                    foreach (var loginSet in loginPolicySet)
                    {
                        var contained = resultLst.FirstOrDefault(x => x.PolicyId == loginSet.PolicyId);

                        if (contained != null)
                        {
                            if (loginSet.Selected.HasValue && loginSet.Selected.Value)
                                continue;

                            resultLst.Remove(contained);
                        }
                        else if (loginSet.Selected.HasValue && loginSet.Selected.Value)
                            resultLst.Add(loginSet);
                    }

                    result.SomeResult = resultLst.ToArray();
                    result.BoolRes = true;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка получения набора политик. ", ex.Message);
            }

            return result;
        }

        public Result<object> AddPolicySet(PolicySetDataContract policySet)
        {
            var result = new Result<object>();

            try
            {
                if (policySet == null)
                    throw new Exception("Новый набор политик не задан");

                using (var ctx = new PolicyProjectEntities())
                {
                    policySet.PolicySetId = ctx.tbl_policy_set.Any() ? ctx.tbl_policy_set.Max(x => x.id) + 1 : 1;
                    var newPolicySet = PolicySetDataContract.FromPolicySetDataContractToTblPolicySet(policySet);
                    ctx.tbl_policy_set.Add(newPolicySet);
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка добавления набора политик. ", ex.Message);
            }

            return result;
        }

        public Result<object> UpdatePolicySet(PolicySetDataContract oldPolicySet, PolicySetDataContract newPolicySet)
        {
            var result = new Result<object>();

            try
            {
                if (oldPolicySet == null)
                    throw new Exception("Текущий набор политик не задан");

                if (newPolicySet == null)
                    throw new Exception("Новый набор политик не задан");

                using (var ctx = new PolicyProjectEntities())
                {
                    var selected = ctx.tbl_policy_set.FirstOrDefault(x => x.id == oldPolicySet.PolicySetId);

                    if (selected == null)
                        throw new Exception("Текущий набор политик не найден");

                    selected.id_policy = newPolicySet.PolicyId;
                    selected.id_group = newPolicySet.GroupId;
                    selected.id_login = newPolicySet.LoginId;
                    selected.selected = newPolicySet.Selected;
                    selected.policy_param = newPolicySet.PolicyParam;
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка изменения наборa политик. ", ex.Message);
            }

            return result;
        }

        public Result<object> DeletePolicySet(PolicySetDataContract policySet)
        {
            var result = new Result<object>();

            try
            {
                if (policySet == null)
                    throw new Exception("Hабор политик не задан");

                using (var ctx = new PolicyProjectEntities())
                {
                    var delPolicySet = ctx.tbl_policy_set.FirstOrDefault(x => x.id == policySet.PolicySetId);
                    ctx.tbl_policy_set.Remove(delPolicySet);
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка удаления наборa политик. ", ex.Message);
            }

            return result;
        }

        public Result<object> UpdatePolicySetForGroup(int groupId, PolicySetDataContract[] policySetList)
        {
            var result = new Result<object>();

            using (var transaction = new TransactionScope())
            {
                try
                {
                    if (groupId < 1)
                        throw new Exception("Не выбрана группа");

                    using (var ctx = new PolicyProjectEntities())
                    {
                        var groupPolicySet = ctx.tbl_policy_set.Where(x => x.id_group == groupId);
                        long newPolicySetId = 0;

                        for (var i = 0; i < policySetList.Length; i++)
                        {
                            var containedGroup =
                                groupPolicySet.FirstOrDefault(x => x.id_policy == policySetList[i].PolicyId);

                            if (policySetList[i].Selected.HasValue && policySetList[i].Selected.Value)
                            {
                                if (containedGroup != null)
                                    continue;

                                if (newPolicySetId == 0)
                                    newPolicySetId = ctx.tbl_policy_set.Any()
                                        ? ctx.tbl_policy_set.Max(x => x.id) + 1
                                        : 1;
                                else
                                    newPolicySetId++;

                                var newPolicySet = new tbl_policy_set
                                {
                                    id = newPolicySetId,
                                    id_policy = policySetList[i].PolicyId,
                                    id_group = groupId,
                                    policy_param = policySetList[i].PolicyParam
                                };
                                ctx.tbl_policy_set.Add(newPolicySet);
                            }
                            else
                            {
                                if (containedGroup == null)
                                    continue;

                                var deletedPolicySet =
                                    ctx.tbl_policy_set.FirstOrDefault(x => x.id == containedGroup.id);
                                ctx.tbl_policy_set.Remove(deletedPolicySet);
                            }
                        }

                        result.BoolRes = ctx.SaveChanges() == policySetList.Length;

                        if (result.BoolRes)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    transaction.Dispose();
                    result.BoolRes = false;
                    result.ErrorRes = string.Concat("Ошибка сохранения набора политик группы. ", ex.Message);
                }
            }

            return result;
        }

        public Result<object> UpdatePolicySetForLogin(long loginId, PolicySetDataContract[] policySetList)
        {
            var result = new Result<object>();

            using (var transaction = new TransactionScope())
            {
                try
                {
                    if (loginId < 1)
                        throw new Exception("Не выбран логин");

                    using (var ctx = new PolicyProjectEntities())
                    {
                        var groupId = ctx.tbl_login.FirstOrDefault(x => x.id == loginId).id_group;
                        var groupPolicySet = ctx.tbl_policy_set.Where(x => x.id_group == groupId);
                        var policySetForLoginInTbl = ctx.tbl_policy_set.Where(x => x.id_login == loginId);
                        long newPolicySetId = 0;

                        for (var i = 0; i < policySetList.Length; i++)
                        {
                            var containedGroup =
                                groupPolicySet.FirstOrDefault(x => x.id_policy == policySetList[i].PolicyId);
                            var containedInTbl =
                                policySetForLoginInTbl.FirstOrDefault(x => x.id_policy == policySetList[i].PolicyId);

                            if (policySetList[i].Selected.HasValue && policySetList[i].Selected.Value)
                            {
                                if (containedGroup == null)
                                {
                                    if (containedInTbl != null)
                                    {
                                        containedInTbl.id_policy = policySetList[i].PolicyId;
                                        containedInTbl.id_login = policySetList[i].LoginId;
                                        containedInTbl.selected = policySetList[i].Selected;
                                        containedInTbl.policy_param = policySetList[i].PolicyParam;
                                    }
                                    else
                                    {
                                        if (newPolicySetId == 0)
                                            newPolicySetId = ctx.tbl_policy_set.Any()
                                                ? ctx.tbl_policy_set.Max(x => x.id) + 1
                                                : 1;
                                        else
                                            newPolicySetId++;

                                        var newPolicySet = new tbl_policy_set
                                        {
                                            id = newPolicySetId,
                                            id_policy = policySetList[i].PolicyId,
                                            id_login = policySetList[i].LoginId,
                                            selected = policySetList[i].Selected,
                                            policy_param = policySetList[i].PolicyParam
                                        };
                                        ctx.tbl_policy_set.Add(newPolicySet);
                                    }
                                }
                                else
                                {
                                    if (containedInTbl == null)
                                        continue;

                                    var deletedPolicySet =
                                        ctx.tbl_policy_set.FirstOrDefault(x => x.id == containedInTbl.id);
                                    ctx.tbl_policy_set.Remove(deletedPolicySet);
                                }
                            }
                            else
                            {
                                if (containedGroup == null)
                                {
                                    if (containedInTbl == null)
                                        continue;

                                    var deletedPolicySet =
                                        ctx.tbl_policy_set.FirstOrDefault(x => x.id == containedInTbl.id);
                                    ctx.tbl_policy_set.Remove(deletedPolicySet);
                                }
                                else
                                {
                                    if (containedInTbl != null)
                                    {
                                        containedInTbl.id_policy = policySetList[i].PolicyId;
                                        containedInTbl.id_login = policySetList[i].LoginId;
                                        containedInTbl.selected = policySetList[i].Selected;
                                        containedInTbl.policy_param = policySetList[i].PolicyParam;
                                    }
                                    else
                                    {
                                        if (newPolicySetId == 0)
                                            newPolicySetId = ctx.tbl_policy_set.Any()
                                                ? ctx.tbl_policy_set.Max(x => x.id) + 1
                                                : 1;
                                        else
                                            newPolicySetId++;

                                        var newPolicySet = new tbl_policy_set
                                        {
                                            id = newPolicySetId,
                                            id_policy = policySetList[i].PolicyId,
                                            id_login = policySetList[i].LoginId,
                                            selected = policySetList[i].Selected,
                                            policy_param = policySetList[i].PolicyParam
                                        };
                                        ctx.tbl_policy_set.Add(newPolicySet);
                                    }
                                }
                            }
                        }

                        result.BoolRes = ctx.SaveChanges() == policySetList.Length;

                        if (result.BoolRes)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    transaction.Dispose();
                    result.BoolRes = false;
                    result.ErrorRes = string.Concat("Ошибка сохранения набора политик логина. ", ex.Message);
                }
            }

            return result;
        }

        public Result<Dictionary<int, int[]>> GetGroupIdAndPolicyIdDct()
        {
            var result = new Result<Dictionary<int, int[]>>();

            try
            {
                using (var ctx = new PolicyProjectEntities())
                {
                    var policySets = from policySet in ctx.tbl_policy_set
                                     where policySet.id_group != null
                                     group policySet by policySet.id_group
                        into selectedPolicySet
                                     select
                                         new
                                         {
                                             GroupId = selectedPolicySet.Key,
                                             PolicySetId = selectedPolicySet.Select(x => x.id_policy)
                                         };
                    var resultDct = new Dictionary<int, int[]>();

                    foreach (var ps in policySets)
                        resultDct.Add(ps.GroupId.Value, ps.PolicySetId.ToArray());

                    result.SomeResult = resultDct;
                    result.BoolRes = true;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка получения наборa политик. ", ex.Message);
            }

            return result;
        }
    }
}