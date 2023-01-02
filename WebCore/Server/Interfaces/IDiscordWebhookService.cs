using Discord;
using GlobalInfraction.WebCore.Shared.Enums;

namespace GlobalInfraction.WebCore.Server.Interfaces;

public interface IDiscordWebhookService
{
    Task CreateInfractionHook(InfractionScope scope, InfractionType infractionType, Guid infractionGuid, string identity, string username, string reason);
    Task CreateIssueHook(Guid instanceGuid, string ipOnRecord, string incomingIp);
}
