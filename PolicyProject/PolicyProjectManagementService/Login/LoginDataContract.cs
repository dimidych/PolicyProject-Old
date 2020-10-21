using System;
using System.Runtime.Serialization;

namespace PolicyProjectManagementService
{
    [DataContract]
    public class LoginDataContract
    {
        [DataMember(Order = 0)]
        public long LoginId { get; set; }

        [DataMember(Order = 1)]
        public long UserId { get; set; }

        [DataMember(Order = 2)]
        public int GroupId { get; set; }

        [DataMember(Order = 3)]
        public string Certificate { get; set; }

        [DataMember(Order = 4)]
        public string Login { get; set; }

        [DataMember(Order = 5)]
        public string Password { get; set; }

        [DataMember(Order = 6)]
        public string UserName { get; set; }

        [DataMember(Order = 7)]
        public string GroupName { get; set; }

        public static LoginDataContract FromTblLoginToLoginDataContract(tbl_login loginTblData)
        {
            if (loginTblData.id < 1 || loginTblData.id_user < 1 || loginTblData.id_group < 1)
                return null;

            return new LoginDataContract
            {
                LoginId = loginTblData.id,
                Login = loginTblData.login,
                Password = loginTblData.pwd,
                Certificate = loginTblData.certificate,
                UserId = loginTblData.id_user,
                GroupId = loginTblData.id_group
            };
        }

        public static tbl_login FromLoginDataContractToTblLogin(LoginDataContract loginContractData)
        {
            if (loginContractData.LoginId < 1 || loginContractData.UserId < 1 || loginContractData.GroupId < 1)
                return null;

            return new tbl_login
            {
                id = loginContractData.LoginId,
                login = loginContractData.Login,
                pwd = loginContractData.Password,
                certificate = loginContractData.Certificate,
                id_user = loginContractData.UserId,
                id_group = loginContractData.GroupId
            };
        }

        public static bool Compare(LoginDataContract obj1, LoginDataContract obj2)
        {
            if (obj1 == null && obj2 == null)
                return true;

            if (obj1 == null && obj2 != null)
                return false;

            if (obj1 != null && obj2 == null)
                return false;

            return obj1.LoginId == obj2.LoginId && obj1.UserId == obj2.UserId && obj1.GroupId == obj2.GroupId
                   &&
                   string.Equals(obj1.Certificate.Trim(), obj2.Certificate.Trim(),
                       StringComparison.CurrentCultureIgnoreCase)
                   && string.Equals(obj1.Login.Trim(), obj2.Login.Trim(), StringComparison.CurrentCultureIgnoreCase);
        }
    }
}