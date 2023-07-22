using BanHubData.Enums;

namespace BanHub.WebCore.Client.Utilities;

public static class ExtensionMethods
{
    public static string GetRoleName(this string role)
    {
        return role switch
        {
            "Web_User" => "User",
            "Web_Admin" => "Admin",
            "Web_SuperAdmin" => "Super Admin",
            "Community_User" => "User",
            "Community_Trusted" => "Trusted",
            "Community_Moderator" => "Moderator",
            "Community_Administrator" => "Admin",
            "Community_SeniorAdmin" => "Senior Admin",
            "Community_Owner" => "Owner",
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
}
