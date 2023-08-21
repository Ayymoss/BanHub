using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Commands.PlayerProfile;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Player;

public class GetProfileHandler : IRequestHandler<GetProfileCommand, Shared.Models.PlayerProfileView.Player?>
{
    private readonly DataContext _context;

    public GetProfileHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Shared.Models.PlayerProfileView.Player?> Handle(GetProfileCommand request, CancellationToken cancellationToken)
    {
        var lastServer = await _context.ServerConnections
            .Where(x => x.Player.Identity == request.Identity)
            .OrderByDescending(x => x.Connected)
            .Select(x=>new
            {
                x.Server.ServerName,
                 x.Server.Community.CommunityName
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);
        
        var entity = await _context.Players
            .Where(profile => profile.Identity == request.Identity)
            .Select(profile => new BanHub.WebCore.Shared.Models.PlayerProfileView.Player
            {
                Identity = profile.Identity,
                UserName = profile.CurrentAlias.Alias.UserName,
                HeartBeat = profile.Heartbeat,
                IpAddress = request.Privileged ? profile.CurrentAlias.Alias.IpAddress : null,
                Connected = profile.Heartbeat + TimeSpan.FromMinutes(5) > DateTimeOffset.UtcNow,
                TotalConnections = profile.TotalConnections,
                PlayTime = profile.PlayTime,
                Created = profile.Created,
                IsGloballyBanned = profile.Penalties.Where(x => x.PenaltyType == PenaltyType.Ban)
                    .Where(x => x.PenaltyStatus == PenaltyStatus.Active)
                    .Any(x => x.PenaltyScope == PenaltyScope.Global),
                CommunityRole = profile.CommunityRole,
                WebRole = profile.WebRole,
                LastConnectedServerName = lastServer != null ? lastServer.ServerName : null,
                LastConnectedCommunityName = lastServer != null ? lastServer.CommunityName : null,
                PenaltyCount = profile.Penalties.Count,
                NoteCount = profile.Notes.Count(x => !x.IsPrivate || request.Privileged),
                ChatCount = profile.Chats.Count
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return entity;
    }
}
