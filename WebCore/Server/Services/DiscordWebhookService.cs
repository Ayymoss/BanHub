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
        var color = createPenaltyEvent.PenaltyType switch
        {
            PenaltyType.Unban => Color.Blue,
            PenaltyType.Unmute => Color.Blue,
            PenaltyType.Report => Color.Default,
            PenaltyType.Warning => Color.Default,
            PenaltyType.TempMute => Color.LightOrange,
            PenaltyType.Mute => Color.LightOrange,
            PenaltyType.Kick => Color.LightOrange,
            PenaltyType.TempBan => Color.Orange,
            PenaltyType.Ban => Color.Red,
            _ => Color.Default
        };

        if (createPenaltyEvent.Scope is PenaltyScope.Global) color = Color.DarkRed;

        var embedBuilder = new EmbedBuilder
        {
            Title = $"Penalty: {createPenaltyEvent.PenaltyType}",
            Description = "Click the link to view the penalty.\n" +
                          $"**Profile:** [View Profile](https://BanHub.gg/Profile/{createPenaltyEvent.Identity})\n" +
                          $"**Penalty ID:** {createPenaltyEvent.PenaltyGuid}\n" +
                          $"**Identity:** {createPenaltyEvent.Identity}\n" +
                          $"**Username:** {createPenaltyEvent.Username}\n" +
                          $"**Reason:** {createPenaltyEvent.Reason}",
            Color = color
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
