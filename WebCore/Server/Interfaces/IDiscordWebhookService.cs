using BanHub.WebCore.Shared.Enums;

namespace BanHub.WebCore.Server.Interfaces;

public interface IDiscordWebhookService
{
    Task CreatePenaltyHook(PenaltyScope scope, PenaltyType penaltyType, Guid penaltyGuid, string identity, string username, string reason);
    Task CreateIssueHook(Guid instanceGuid, string ipOnRecord, string incomingIp);
    Task CreateAdminActionHook(string message);
}
