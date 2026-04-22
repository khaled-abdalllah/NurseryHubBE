namespace NurseryHub.Permissions;

public static class NurseryHubPermissions
{
    public const string GroupName = "NurseryHub";

    public static class Nurseries
    {
        public const string Default = GroupName + ".Nurseries";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Governorates
    {
        public const string Default = GroupName + ".Governorates";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Cities
    {
        public const string Default = GroupName + ".Cities";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class NurseryBranches
    {
        public const string Default = GroupName + ".NurseryBranches";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
}
