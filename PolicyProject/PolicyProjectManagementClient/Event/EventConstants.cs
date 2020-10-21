namespace PolicyProjectManagementClient
{
    internal static class EventConstants
    {
        public const int EVENT_ADD_USER = 6;
        public const int EVENT_UPD_USER = 8;
        public const int EVENT_DEL_USER = 7;

        public const int EVENT_ADD_LOGIN = 9;
        public const int EVENT_UPD_LOGIN = 10;
        public const int EVENT_DEL_LOGIN = 11;

        public const int EVENT_ADD_DEVICE = 12;
        public const int EVENT_UPD_DEVICE = 13;
        public const int EVENT_DEL_DEVICE = 14;

        public const int EVENT_UNLOAD_CERTIFICATE = 15;

        public const int EVENT_ADD_EVENT = 16;
        public const int EVENT_UPD_EVENT = 17;
        public const int EVENT_DEL_EVENT = 18;

        public const int EVENT_ADD_GROUP = 19;
        public const int EVENT_UPD_GROUP = 20;
        public const int EVENT_DEL_GROUP = 21;

        public const int EVENT_LOGON = 22;
        public const int EVENT_LOGOUT = 30;

        public const int EVENT_ADD_POLICY = 23;
        public const int EVENT_UPD_POLICY = 24;
        public const int EVENT_DEL_POLICY = 25;

        public const int EVENT_ASSIGN_lOGIN_DEVICE = 26;
        public const int EVENT_UNASSIGN_lOGIN_DEVICE = 27;

        public const int EVENT_ASSIGN_lOGIN_POLICY = 28;
        public const int EVENT_UNASSIGN_lOGIN_POLICY = 29;

        public const int EVENT_ERROR = 31;

        public const int EVENT_DEL_LOG = 32;
    }
}