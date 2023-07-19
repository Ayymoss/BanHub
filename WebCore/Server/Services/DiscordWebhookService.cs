using BanHub.WebCore.Server.Events.DiscordWebhook;
using BanHub.WebCore.Server.Interfaces;
using BanHubData.Enums;
using Discord;
using Discord.Webhook;

namespace BanHub.WebCore.Server.Services;

public class DiscordWebhookService
{
    private readonly Configuration _configuration;

    public DiscordWebhookService(Configuration configuration)
    {
        _configuration = configuration;
        
        IDiscordWebhookSubscriptions.CreateIssue += OnCreateIssueHookAsync;
        IDiscordWebhookSubscriptions.CreatePenalty += OnCreatePenaltyHookAsync;
        IDiscordWebhookSubscriptions.CreateAdminAction += OnCreateAdminActionHookAsync;
    }

    private async Task OnCreatePenaltyHookAsync(CreatePenaltyEvent createPenaltyEvent, CancellationToken token)
    {
        var embedBuilder = new EmbedBuilder
        {
            Title = $"Penalty: {createPenaltyEvent.PenaltyType}",
            Description = "Click the link to view the penalty.\n" +
                          $"**Profile:** [{createPenaltyEvent.Username}](https://BanHub.gg/Players/{createPenaltyEvent.TargetIdentity})\n" +
                          $"**Penalty ID:** {createPenaltyEvent.PenaltyGuid}\n" +
                          $"**Reason:** {createPenaltyEvent.Reason}\n\n" +
                          $"**Community:** [{createPenaltyEvent.InstanceName}](https://BanHub.gg/Communities/{createPenaltyEvent.InstanceGuid})",
            Color = Color.DarkRed
        };

        await SendWebhook(embedBuilder.Build(), _configuration.PenaltyWebHook);
    } 
    
    private async Task OnCreateIssueHookAsync(CreateIssueEvent createIssueEvent, CancellationToken token)
    {
        var embedBuilder = new EmbedBuilder
        {
            Title = $"IP Mismatch: {createIssueEvent.InstanceGuid}",
            Description = "IP Mismatch Issue Raised\n" +
                          $"**Record IP:** {createIssueEvent.InstanceIp}\n" +
                          $"**Incoming IP:** {createIssueEvent.IncomingIp}",
            Color = Color.DarkRed
        };

        await SendWebhook(embedBuilder.Build(), _configuration.InstanceWebHook);
    }

    private async Task OnCreateAdminActionHookAsync(CreateAdminActionEvent createAdminActionEvent, CancellationToken token)
    {
        var embedBuilder = new EmbedBuilder
        {
            Title = $"Admin Action - {createAdminActionEvent.Title}",
            Description = createAdminActionEvent.Message,
            Color = Color.DarkRed
        };

        await SendWebhook(embedBuilder.Build(), _configuration.AdminActionWebHook);
    }

    private static async Task SendWebhook(Embed embed, string webhook)
    {
        var client = new DiscordWebhookClient(webhook);
        await client.SendMessageAsync(embeds: new[] {embed});
    }
}
