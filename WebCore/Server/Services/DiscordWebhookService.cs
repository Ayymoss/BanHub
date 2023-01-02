using Discord;
using Discord.Webhook;
using GlobalInfraction.WebCore.Server.Interfaces;
using GlobalInfraction.WebCore.Shared.Enums;

namespace GlobalInfraction.WebCore.Server.Services;

public class DiscordWebhookService : IDiscordWebhookService
{
    private readonly Configuration _configuration;

    public DiscordWebhookService(Configuration configuration)
    {
        _configuration = configuration;
    }

    public async Task CreateInfractionHook(InfractionScope scope, InfractionType infractionType, Guid infractionGuid, string identity, string
        username, string reason)
    {
        Color color;
        switch (infractionType)
        {
            case InfractionType.Warn:
            case InfractionType.Mute:
            case InfractionType.Kick:
                color = Color.LightOrange;
                break;
            case InfractionType.Unban:
                color = Color.Blue;
                break;
            case InfractionType.TempBan:
                color = Color.Orange;
                break;
            case InfractionType.Ban:
            default:
                color = Color.Red;
                break;
        }

        if (scope == InfractionScope.Global) color = Color.DarkRed;

        var embedBuilder = new EmbedBuilder
        {
            Title = $"Infraction: {infractionType}",
            Description = "Click the link to view the infraction.\n" +
                          $"**Infraction:** [View Infraction](https://globalinfractions.com/Infractions?search={identity})\n" +
                          $"**Identity:** {identity}\n" +
                          $"**Username:** {username}\n" +
                          $"**Reason:** {reason}",
            Color = color
        };

        await SendWebhook(embedBuilder.Build(), _configuration.InfractionWebHook);
    } 
    
    public async Task CreateIssueHook(Guid instanceGuid, string ipOnRecord, string incomingIp)
    {
        var embedBuilder = new EmbedBuilder
        {
            Title = $"IP Mismatch: {instanceGuid}",
            Description = "IP mismatch issue raised\n" +
                          $"**Record IP:** {ipOnRecord}" +
                          $"**Incoming IP:** {incomingIp}",
            Color = Color.DarkRed
        };

        await SendWebhook(embedBuilder.Build(), _configuration.InstanceWebHook);
    }

    private async Task SendWebhook(Embed embed, string webhook)
    {
        var client = new DiscordWebhookClient(webhook);
        await client.SendMessageAsync(embeds: new[] {embed});
    }
}
