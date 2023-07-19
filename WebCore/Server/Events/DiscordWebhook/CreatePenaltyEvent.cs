using BanHubData.Enums;

namespace BanHub.WebCore.Server.Events.DiscordWebhook;

public class CreatePenaltyEvent : CoreEvent
{
    public PenaltyScope Scope { get; set; }
    public PenaltyType PenaltyType { get; set; }
    public Guid PenaltyGuid { get; set; }
    public string Identity { get; set; }
    public string Username { get; set; }
    public string Reason { get; set; }
    public string InstanceName { get; set; }
    public Guid InstanceGuid { get; set; }
}
