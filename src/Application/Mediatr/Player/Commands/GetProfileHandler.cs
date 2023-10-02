using BanHub.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BanHub.Application.Mediatr.Player.Commands;

public class GetProfileHandler(IServerConnectionRepository serverConnectionRepository, IPlayerRepository playerRepository, ISender sender,
    ILogger logger) : IRequestHandler<GetProfileCommand, DTOs.WebView.PlayerProfileView.Player?>
{
    public async Task<DTOs.WebView.PlayerProfileView.Player?> Handle(GetProfileCommand request, CancellationToken cancellationToken)
    {
        var lastServer = await serverConnectionRepository.GetLastConnectedServerAsync(request.Identity, cancellationToken);
        var profile = await playerRepository.GetPlayerProfileViewAsync(request.Identity, cancellationToken);
        if (profile is null)
        {
            logger.LogWarning("Player profile not found for {Identity}", request.Identity);
            return null;
        }

        var isGloballyBanned = await playerRepository.IsGloballyBannedAsync(request.Identity, cancellationToken);
        var player = new DTOs.WebView.PlayerProfileView.Player
        {
            Identity = profile.Identity,
            UserName = profile.UserName,
            Heartbeat = profile.Heartbeat,
            IpAddress = request.Privileged ? profile.IpAddress : null,
            Connected = profile.Connected,
            TotalConnections = profile.TotalConnections,
            PlayTime = profile.PlayTime,
            Created = profile.Created,
            IsGloballyBanned = isGloballyBanned,
            CommunityRole = profile.CommunityRole,
            WebRole = profile.WebRole,
            LastConnectedServerName = lastServer?.ServerName,
            LastConnectedCommunityName = lastServer?.CommunityName,
            PenaltyCount = profile.PenaltyCount,
            // NoteCount = profile.Notes.Count(x => !x.IsPrivate || privileged), // Needs to be done in the query - but we return the count
            NoteCount = profile.NoteCount,
            ChatCount = profile.ChatCount
        };

        var sentiment = await sender.Send(new GetPlayerChatSentimentScoreCommand {Identity = request.Identity}, cancellationToken);
        player.ChatSentimentScore = sentiment;
        return player;
    }
}
