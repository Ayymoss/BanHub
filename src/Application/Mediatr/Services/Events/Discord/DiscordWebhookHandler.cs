using BanHub.Application.Utilities;
using Discord;
using Discord.Webhook;
using MediatR;

namespace BanHub.Application.Mediatr.Services.Events.Discord;

// TODO: Move DiscordNet to Infrastructure layer

public class DiscordWebhookHandler(Configuration configuration)
    : INotificationHandler<CreateAdminActionNotification>, INotificationHandler<CreateIssueNotification>,
        INotificationHandler<CreatePenaltyNotification>
{
    public async Task Handle(CreateAdminActionNotification notification, CancellationToken cancellationToken)
    {
        var embedBuilder = new EmbedBuilder
        {
            Title = $"Admin Action - {notification.Title}",
            Description = notification.Message,
            Color = Color.DarkRed
        };

        await SendWebhook(embedBuilder.Build(), configuration.AdminActionWebHook);
    }

    public async Task Handle(CreateIssueNotification notification, CancellationToken cancellationToken)
    {
        var embedBuilder = new EmbedBuilder
        {
            Title = $"IP Mismatch: {notification.CommunityGuid}",
            Description = "IP Mismatch Issue Raised\n" +
                          $"**Record IP:** {notification.CommunityIp}\n" +
                          $"**Incoming IP:** {notification.IncomingIp}",
            Color = Color.DarkRed
        };

        await SendWebhook(embedBuilder.Build(), configuration.CommunityWebHook);
    }

    public async Task Handle(CreatePenaltyNotification notification, CancellationToken cancellationToken)
    {
        var embedBuilder = new EmbedBuilder
        {
            Title = $"Penalty: {notification.PenaltyType}",
            Description = "Click the link to view the penalty.\n" +
                          $"**Profile:** [{notification.Username}](https://BanHub.gg/Players/{notification.TargetIdentity})\n" +
                          $"**Penalty ID:** {notification.PenaltyGuid}\n" +
                          $"**Reason:** {notification.Reason}\n\n" +
                          $"**Community:** [{notification.CommunityName}](https://BanHub.gg/Communities/{notification.CommunityGuid})",
            Color = Color.DarkRed
        };

        await SendWebhook(embedBuilder.Build(), configuration.PenaltyWebHook);
    }

    private static async Task SendWebhook(Embed embed, string webhook)
    {
        var client = new DiscordWebhookClient(webhook);
        await client.SendMessageAsync(embeds: new[] {embed});
    }
}
