namespace BanHub.WebCore.Client.Utilities;

public static class ExtensionMethod
{
    public static string GetRoleName(this string role)
    {
        return role switch
        {
            "WebUser" => "User",
            "WebAdmin"  => "Admin",
            "WebSuperAdmin" => "Super Admin",
            "InstanceUser" => "User",
            "InstanceTrusted"  => "Trusted",
            "InstanceModerator"  => "Moderator",
            "InstanceAdministrator"  => "Admin",
            "InstanceSeniorAdmin" => "Senior Admin",
            "InstanceOwner" => "Owner",
            _ => "Unknown"
        };
     
    }
}
