namespace BanHub.WebCore.Server.Utilities;

public class Configuration
{
    public byte Version { get; set; } = 1;
    public int WebBind { get; set; } = 80;
    public string PenaltyWebHook { get; set; }
    public string CommunityWebHook { get; set; }
    public string AdminActionWebHook { get; set; }
    public string SentimentModelDirectory { get; set; }
    public Version PluginVersion { get; set; } = new();
    public DatabaseConfiguration Database { get; set; } = new();
}

public class DatabaseConfiguration
{
    public string HostName { get; set; } = null!;
    public int Port { get; set; }
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Database { get; set; } = null!;
}
