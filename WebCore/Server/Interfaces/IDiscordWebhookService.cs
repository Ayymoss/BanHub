using BanHub.WebCore.Server.Events;
using BanHub.WebCore.Server.Events.DiscordWebhook;

namespace BanHub.WebCore.Server.Interfaces;

public interface IDiscordWebhookSubscriptions
{
    /// <summary>
    /// Raised when a penalty is created
    /// </summary>
    static event Func<CreatePenaltyEvent, CancellationToken, Task>? CreatePenalty;

    /// <summary>
    /// Raised when an issue is created
    /// </summary>
    static event Func<CreateIssueEvent, CancellationToken, Task>? CreateIssue;

    /// <summary>
    /// Raised when an admin action is created
    /// </summary>
    static event Func<CreateAdminActionEvent, CancellationToken, Task>? CreateAdminAction;

    static object? InvokeEvent(CoreEvent coreEvent, CancellationToken token)
    {
        return coreEvent switch
        {
            CreatePenaltyEvent createPenaltyEvent => CreatePenalty?.Invoke(createPenaltyEvent, token),
            CreateIssueEvent createIssueEvent => CreateIssue?.Invoke(createIssueEvent, token),
            CreateAdminActionEvent createAdminActionEvent => CreateAdminAction?.Invoke(createAdminActionEvent, token),
            _ => null
        };
    }
}
