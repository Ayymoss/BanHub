using Data.Enums;

namespace BanHub.WebCore.Server.Interfaces;

public interface IDiscordWebhookService
{
    Task CreatePenaltyHookAsync(PenaltyScope scope, PenaltyType penaltyType, Guid penaltyGuid, string identity, string username, string reason);
    Task CreateIssueHookAsync(Guid instanceGuid, string ipOnRecord, string incomingIp);
    Task CreateAdminActionHookAsync(string title, string message);
}
