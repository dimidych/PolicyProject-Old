using System;
using System.Linq;
using CryptoToolLib;
using Newtonsoft.Json;
using ServiceRealizationAtribute;

namespace PolicyProjectManagementService
{
    [ServiceRealization(
        InterfaceName = "PolicyProjectManagementService.ILoginService",
        Name = "PolicyProjectManagementService.LoginService", OrderToLoad = 0)]
    public class LoginService : ILoginService
    {
        public Result<LoginDataContract[]> GetLogin(LoginDataContract loginFilter)
        {
            var result = new Result<LoginDataContract[]>();

            try
            {
                using (var ctx = new PolicyProjectEntities())
                {
                    if (loginFilter == null)
                        result.SomeResult =
                            ctx.tbl_login.Select(
                                    loginTblData => new LoginDataContract
                                    {
                                        LoginId = loginTblData.id,
                                        Login = loginTblData.login,
                                        Password = loginTblData.pwd,
                                        Certificate = loginTblData.certificate,
                                        UserId = loginTblData.id_user,
                                        GroupId = loginTblData.id_group,
                                        GroupName =
                                            ctx.tbl_group.FirstOrDefault(group => group.id == loginTblData.id_group)
                                                .group_name,
                                        UserName =
                                            ctx.tbl_user.Where(user => user.id == loginTblData.id_user)
                                                .Select(
                                                    x => (x.first_name + " " + x.last_name + " " + x.middle_name).Trim())
                                                .FirstOrDefault()
                                    })
                                .ToArray();
                    else
                        result.SomeResult = ctx.tbl_login.Where(x => x.id == loginFilter.LoginId
                                                                     ||
                                                                     x.login.Trim()
                                                                         .Equals(loginFilter.Login.Trim(),
                                                                             StringComparison.InvariantCultureIgnoreCase))
                            .Select(loginTblData => new LoginDataContract
                            {
                                LoginId = loginTblData.id,
                                Login = loginTblData.login,
                                Password = loginTblData.pwd,
                                Certificate = loginTblData.certificate,
                                UserId = loginTblData.id_user,
                                GroupId = loginTblData.id_group,
                                GroupName =
                                    ctx.tbl_group.FirstOrDefault(group => group.id == loginTblData.id_group).group_name,
                                UserName =
                                    ctx.tbl_user.Where(user => user.id == loginTblData.id_user)
                                        .Select(x => (x.first_name + " " + x.last_name + " " + x.middle_name).Trim())
                                        .FirstOrDefault()
                            })
                            .ToArray();

                    result.BoolRes = true;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка получения списка логинов. ", ex.Message);
            }

            return result;
        }

        public string GetLoginRest(LoginDataContract loginFilter)
        {
            var queryResult = GetLogin(loginFilter);
            return JsonConvert.SerializeObject(queryResult);
        }

        public Result<string[]> GetCertificate(long loginId)
        {
            var result = new Result<string[]>();

            try
            {
                using (var ctx = new PolicyProjectEntities())
                {
                    var login = ctx.tbl_login.FirstOrDefault(x => x.id == loginId);

                    if (login == null)
                        throw new Exception("Логин не найден");

                    result.SomeResult = new[] {login.login, login.certificate};
                    result.BoolRes = true;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка получения сертификата. ", ex.Message);
            }

            return result;
        }

        public string GetCertificateRest(long loginId)
        {
            var queryResult = GetCertificate(loginId);
            return JsonConvert.SerializeObject(queryResult);
        }

        public Result<object> AddLogin(LoginDataContract login)
        {
            var result = new Result<object>();

            try
            {
                if (login == null)
                    throw new Exception("Новый логин не задан");

                using (var ctx = new PolicyProjectEntities())
                {
                    var selected =
                        ctx.tbl_login.FirstOrDefault(
                            x => x.login.Trim().Equals(login.Login.Trim(), StringComparison.InvariantCultureIgnoreCase));

                    if (selected != null)
                        throw new Exception("Такой логин уже существует");

                    login.LoginId = ctx.tbl_login.Any() ? ctx.tbl_login.Max(x => x.id) + 1 : 1;
                    var newLogin = LoginDataContract.FromLoginDataContractToTblLogin(login);
                    newLogin.certificate = CreateCertificate();
                    ctx.tbl_login.Add(newLogin);
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка добавления логина. ", ex.Message);
            }

            return result;
        }

        public Result<object> UpdateLogin(LoginDataContract oldLogin, LoginDataContract newLogin)
        {
            var result = new Result<object>();

            try
            {
                if (oldLogin == null)
                    throw new Exception("Текущий логин не задан");

                if (newLogin == null)
                    throw new Exception("Новый логин не задан");

                using (var ctx = new PolicyProjectEntities())
                {
                    var selected = ctx.tbl_login.FirstOrDefault(x => x.id == oldLogin.LoginId);

                    if (selected == null)
                        throw new Exception("Текущий логин не найден");

                    selected.login = newLogin.Login;
                    selected.pwd = newLogin.Password;
                    selected.certificate = CreateCertificate();
                    selected.id_user = newLogin.UserId;
                    selected.id_group = newLogin.GroupId;
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка изменения логина. ", ex.Message);
            }

            return result;
        }

        public Result<object> DeleteLogin(LoginDataContract login)
        {
            var result = new Result<object>();

            try
            {
                if (login == null)
                    throw new Exception("Логин не задан");

                using (var ctx = new PolicyProjectEntities())
                {
                    var delLogin = ctx.tbl_login.FirstOrDefault(x => x.id == login.LoginId);
                    ctx.tbl_login.Remove(delLogin);
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка удаления логина. ", ex.Message);
            }

            return result;
        }

        private static string CreateCertificate()
        {
            var cryptoWorker = new CryptoWorker(CryptoSystemType.RSA, string.Empty, false);
            var bytes = cryptoWorker.ExportKeyBlob(true);
            return Convert.ToBase64String(bytes);
            //return string.Concat(bytes.Select(b => Convert.ToString(b, 10)));
        }
    }
}