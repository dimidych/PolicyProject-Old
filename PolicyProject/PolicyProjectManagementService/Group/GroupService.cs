using System;
using System.Linq;
using Newtonsoft.Json;
using ServiceRealizationAtribute;

namespace PolicyProjectManagementService
{
    [ServiceRealization(
        InterfaceName = "PolicyProjectManagementService.IGroupService",
        Name = "PolicyProjectManagementService.GroupService", OrderToLoad = 0)]
    public class GroupService : IGroupService
    {
        public Result<GroupDataContract[]> GetGroup(GroupDataContract groupFilter)
        {
            var result = new Result<GroupDataContract[]>();

            try
            {
                using (var ctx = new PolicyProjectEntities())
                {
                    if (groupFilter == null)
                        result.SomeResult =
                            ctx.tbl_group.Select(
                                groupTblData =>
                                    new GroupDataContract
                                    {
                                        GroupId = groupTblData.id,
                                        GroupName = groupTblData.group_name
                                    }).ToArray();
                    else
                        result.SomeResult = ctx.tbl_group.Where(x => x.id == groupFilter.GroupId)
                            .Select(groupTblData =>
                                new GroupDataContract
                                {
                                    GroupId = groupTblData.id,
                                    GroupName = groupTblData.group_name
                                })
                            .ToArray();

                    result.BoolRes = true;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка получения списка групп. ", ex.Message);
            }

            return result;
        }

        public string GetGroupRest(GroupDataContract groupFilter)
        {
            var queryResult = GetGroup(groupFilter);
            return JsonConvert.SerializeObject(queryResult);
        }

        public Result<object> AddGroup(GroupDataContract group)
        {
            var result = new Result<object>();

            try
            {
                if (group == null)
                    throw new Exception("Новая группа не задана");

                using (var ctx = new PolicyProjectEntities())
                {
                    var selected =
                        ctx.tbl_group.FirstOrDefault(
                            x =>
                                x.group_name.Trim()
                                    .Equals(group.GroupName.Trim(), StringComparison.InvariantCultureIgnoreCase));

                    if (selected != null)
                        throw new Exception("Группа уже существует");

                    group.GroupId = ctx.tbl_group.Any() ? ctx.tbl_group.Max(x => x.id) + 1 : 1;
                    var newGroup = GroupDataContract.FromGroupDataContractToTblGroup(group);
                    ctx.tbl_group.Add(newGroup);
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка добавления группы. ", ex.Message);
            }

            return result;
        }

        public Result<object> UpdateGroup(GroupDataContract oldGroup, GroupDataContract newGroup)
        {
            var result = new Result<object>();

            try
            {
                if (oldGroup == null)
                    throw new Exception("Текущая группа не задана");

                if (newGroup == null)
                    throw new Exception("Новая группа не задана");

                using (var ctx = new PolicyProjectEntities())
                {
                    var selected = ctx.tbl_group.FirstOrDefault(x => x.id == oldGroup.GroupId);

                    if (selected == null)
                        throw new Exception("Текущая группа не найдена");

                    selected.group_name = newGroup.GroupName;
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка изменения группы. ", ex.Message);
            }

            return result;
        }

        public Result<object> DeleteGroup(GroupDataContract group)
        {
            var result = new Result<object>();

            try
            {
                if (group == null)
                    throw new Exception("Группа не задана");

                using (var ctx = new PolicyProjectEntities())
                {
                    var delGroup = ctx.tbl_group.FirstOrDefault(x => x.id == group.GroupId);
                    ctx.tbl_group.Remove(delGroup);
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка удаления группы. ", ex.Message);
            }

            return result;
        }
    }
}