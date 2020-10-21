using System.Runtime.Serialization;

namespace PolicyProjectManagementService
{
    [DataContract]
    public class UserDataContract
    {
        [DataMember(Order = 0)]
        public long UserId { get; set; }

        [DataMember(Order = 1)]
        public string UserLastName { get; set; }

        [DataMember(Order = 2)]
        public string UserFirstName { get; set; }

        [DataMember(Order = 3)]
        public string UserMiddleName { get; set; }

        [DataMember(Order = 4)]
        public string UserName
        {
            get { return string.Concat(UserFirstName, " ", UserLastName, " ", UserMiddleName).Trim(); }
            set { return; }
        }

        public static tbl_user FromUserDataContractToTblUser(UserDataContract userData)
        {
            if (userData.UserId < 1 || string.IsNullOrEmpty(userData.UserLastName))
                return null;

            return new tbl_user
            {
                id = userData.UserId,
                last_name = userData.UserLastName,
                first_name = userData.UserFirstName,
                middle_name = userData.UserMiddleName
            };
        }

        public static UserDataContract FromTblUserToUserDataContract(tbl_user userTblData)
        {
            if (userTblData.id < 1 || string.IsNullOrEmpty(userTblData.last_name))
                return null;

            return new UserDataContract
            {
                UserId = userTblData.id,
                UserLastName = userTblData.last_name,
                UserFirstName = userTblData.first_name,
                UserMiddleName = userTblData.middle_name
            };
        }

        public static bool Compare(UserDataContract obj1, UserDataContract obj2)
        {
            if (obj1 == null && obj2 == null)
                return true;

            if (obj1 == null && obj2 != null)
                return false;

            if (obj1 != null && obj2 == null)
                return false;

            return obj1.UserId == obj2.UserId &&
                   obj1.UserLastName.Trim().ToLower() == obj2.UserLastName.Trim().ToLower()
                   && obj1.UserFirstName.Trim().ToLower() == obj2.UserFirstName.Trim().ToLower()
                   && obj1.UserMiddleName.Trim().ToLower() == obj2.UserMiddleName.Trim().ToLower();
        }
    }

    [DataContract]
    public class ImageDataContract
    {
        [DataMember(Order = 0)]
        public string ImageName { get; set; }

        [DataMember(Order = 1)]
        public byte[] ImageBytes { get; set; }
    }
}