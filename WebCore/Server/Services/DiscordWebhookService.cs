﻿using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Shared.Enums;
using Discord;
using Discord.Webhook;

namespace BanHub.WebCore.Server.Services;

public class DiscordWebhookService : IDiscordWebhookService
{
    private readonly Configuration _configuration;

    public DiscordWebhookService(Configuration configuration)
    {
        _configuration = configuration;
    }

    public async Task CreatePenaltyHook(PenaltyScope scope, PenaltyType penaltyType, Guid penaltyGuid, string identity, 
        string username, string reason)
    {
        Color color;
        switch (penaltyType)
        {
            case PenaltyType.Unban:
                color = Color.Blue;
                break;
            case PenaltyType.Warn:
                color = Color.Default;
                break;
            case PenaltyType.Mute:
            case PenaltyType.Kick:
                color = Color.LightOrange;
                break;
            case PenaltyType.TempBan:
                color = Color.Orange;
                break;
            case PenaltyType.Ban:
            default:
                color = Color.Red;
                break;
        }

        if (scope == PenaltyScope.Global) color = Color.DarkRed;

        var embedBuilder = new EmbedBuilder
        {
            Title = $"Penalty: {penaltyType}",
            Description = "Click the link to view the penalty.\n" +
                          $"**Penalty:** [View Profile](https://BanHub.gg/Profile/{identity})\n" +
                          $"**Identity:** {identity}\n" +
                          $"**Username:** {username}\n" +
                          $"**Reason:** {reason}",
            Color = color
        };

        await SendWebhook(embedBuilder.Build(), _configuration.PenaltyWebHook);
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
