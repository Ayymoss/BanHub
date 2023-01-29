using BanHub.WebCore.Shared.Enums;

namespace BanHub.WebCore.Server.Interfaces;

public interface IDiscordWebhookService
{
    Task CreateInfractionHook(InfractionScope scope, InfractionType infractionType, Guid infractionGuid, string identity, string username, string reason);
    Task CreateIssueHook(Guid instanceGuid, string ipOnRecord, string incomingIp);
}
