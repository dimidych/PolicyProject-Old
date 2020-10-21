namespace PolicyProjectManagementClient
{
    public class LoginEventArgs
    {
        public LoginEventArgs(long loginId, long deviceId, string device, string login, string loginName)
        {
            LoginId = loginId;
            DeviceId = deviceId;
            Device = device;
            Login = login;
            LoginName = loginName;
        }

        public long LoginId { get; }

        public long DeviceId { get; }

        public string Device { get; }

        public string Login { get; }

        public string LoginName { get; }

    }
}