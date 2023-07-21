namespace BanHub.WebCore.Server.Events.DiscordWebhook;

public class CreateIssueEvent : CoreEvent
{
    public Guid CommunityGuid { get; set; }
    public string CommunityIp { get; set; }
    public string IncomingIp { get; set; }
}
