using System;
using System.Linq;
using Newtonsoft.Json;
using ServiceRealizationAtribute;

namespace PolicyProjectManagementService
{
    [ServiceRealization(
        InterfaceName = "PolicyProjectManagementService.IUserService",
        Name = "PolicyProjectManagementService.UserService", OrderToLoad = 0)]
    public class UserService : IUserService
    {
        public Result<UserDataContract[]> GetUser(UserDataContract userFilter)
        {
            var result = new Result<UserDataContract[]>();

            try
            {
                using (var ctx = new PolicyProjectEntities())
                {
                    if (userFilter == null)
                        result.SomeResult =
                            ctx.tbl_user.Select(userTblData => new UserDataContract
                            {
                                UserId = userTblData.id,
                                UserLastName = userTblData.last_name,
                                UserFirstName = userTblData.first_name,
                                UserMiddleName = userTblData.middle_name
                            }).ToArray();
                    else
                        result.SomeResult =
                            ctx.tbl_user.Where(x => x.id == userFilter.UserId)
                                .Select(userTblData => new UserDataContract
                                {
                                    UserId = userTblData.id,
                                    UserLastName = userTblData.last_name,
                                    UserFirstName = userTblData.first_name,
                                    UserMiddleName = userTblData.middle_name
                                })
                                .ToArray();

                    result.BoolRes = true;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка получения списка пользователей. ", ex.Message);
            }

            return result;
        }

        public string GetUserRest(UserDataContract userFilter)
        {
            var queryResult = GetUser(userFilter);
            return JsonConvert.SerializeObject(queryResult);
        }

        public string GetUserRestJson(string jsonFilter)
        {
            var userFilter = JsonWorker.JsonWorker<UserDataContract>.Deserialize(jsonFilter);
            var queryResult = GetUser(userFilter);
            return JsonConvert.SerializeObject(queryResult);
        }

        public Result<object> AddUser(UserDataContract user)
        {
            var result = new Result<object>();

            try
            {
                if (user == null)
                    throw new Exception("Новый пользователь не задан");

                using (var ctx = new PolicyProjectEntities())
                {
                    user.UserId = ctx.tbl_user.Any() ? ctx.tbl_user.Max(x => x.id) + 1 : 1;
                    var newUser = UserDataContract.FromUserDataContractToTblUser(user);
                    ctx.tbl_user.Add(newUser);
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка добавления пользователя. ", ex.Message);
            }

            return result;
        }

        public Result<object> UpdateUser(UserDataContract oldUser, UserDataContract newUser)
        {
            var result = new Result<object>();

            try
            {
                if (oldUser == null)
                    throw new Exception("Текущий пользователь не задан");

                if (newUser == null)
                    throw new Exception("Новый пользователь не задан");

                using (var ctx = new PolicyProjectEntities())
                {
                    var selected = ctx.tbl_user.FirstOrDefault(x => x.id == oldUser.UserId);

                    if (selected == null)
                        throw new Exception("Текущий пользователь не найден");

                    selected.first_name = newUser.UserFirstName;
                    selected.last_name = newUser.UserLastName;
                    selected.middle_name = newUser.UserMiddleName;
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка изменения пользователя. ", ex.Message);
            }

            return result;
        }

        public Result<object> DeleteUser(UserDataContract user)
        {
            var result = new Result<object>();

            try
            {
                if (user == null)
                    throw new Exception("Пользователь не задан");

                using (var ctx = new PolicyProjectEntities())
                {
                    var delUser = ctx.tbl_user.FirstOrDefault(x => x.id == user.UserId);
                    ctx.tbl_user.Remove(delUser);
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка удаления пользователя. ", ex.Message);
            }

            return result;
        }
    }
}