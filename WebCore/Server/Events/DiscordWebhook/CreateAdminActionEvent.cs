namespace BanHub.WebCore.Server.Events.DiscordWebhook;

public class CreateAdminActionEvent : CoreEvent
{
    public string Title { get; set; }
    public string Message { get; set; }
}
