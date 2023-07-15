namespace BanHub.WebCore.Server.Events.DiscordWebhook;

public class CreateIssueEvent : CoreEvent
{
    public Guid InstanceGuid { get; set; }
    public string InstanceIp { get; set; }
    public string IncomingIp { get; set; }
}
