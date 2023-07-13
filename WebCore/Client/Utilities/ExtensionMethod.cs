using BanHubData.Domains;
using BanHubData.Enums;

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
    
    public static string GetGameName(this Game game)
    {
        return game switch
        {
            Game.COD => "Call of Duty",
            Game.IW3 => "Modern Warfare",
            Game.IW4 => "Modern Warfare 2",
            Game.IW5 => "Modern Warfare 3",
            Game.IW6 => "Modern Warfare 2",
            Game.T4 => "World at War",
            Game.T5 => "Black Ops",
            Game.T6 => "Black Ops 2",
            Game.T7 => "Black Ops 3",
            Game.SHG1 => "Advanced Warfare",
            Game.CSGO => "Counter-Strike: Global Offensive",
            Game.H1 => "Modern Warfare Remastered",
            _ => "Unknown"
        };
    }

    public static bool IsGlobalBanned(this Player entity) =>
        entity.Penalties?.Any(x => x is {PenaltyStatus: PenaltyStatus.Active, PenaltyScope: PenaltyScope.Global}) ?? false;
}
