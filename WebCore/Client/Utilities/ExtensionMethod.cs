using BanHub.WebCore.Shared.DTOs;
using BanHub.WebCore.Shared.Enums;

namespace BanHub.WebCore.Client.Utilities;

public static class ExtensionMethod
{
    public static string GetRoleName(this string role)
    {
        return role switch
        {
            "WebUser" => "User",
            "WebAdmin" => "Admin",
            "WebSuperAdmin" => "Super Admin",
            "InstanceUser" => "User",
            "InstanceTrusted" => "Trusted",
            "InstanceModerator" => "Moderator",
            "InstanceAdministrator" => "Admin",
            "InstanceSeniorAdmin" => "Senior Admin",
            "InstanceOwner" => "Owner",
            _ => "Unknown"
        };
    }

    public static bool IsGlobalBanned(this EntityDto entity) =>
        entity.Penalties?.Any(x => x is {PenaltyStatus: PenaltyStatus.Active, PenaltyScope: PenaltyScope.Global}) ?? false;
}
