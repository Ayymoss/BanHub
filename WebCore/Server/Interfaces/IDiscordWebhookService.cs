using Discord;
using GlobalInfraction.WebCore.Shared.Enums;

namespace GlobalInfraction.WebCore.Server.Interfaces;

public interface IDiscordWebhookService
{
    public Task CreateWebhook(InfractionScope scope, InfractionType infractionType, Guid infractionGuid, string identity, 
        string username, string reason);
}
