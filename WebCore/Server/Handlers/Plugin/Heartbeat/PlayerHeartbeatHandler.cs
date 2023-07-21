using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Models;
using BanHubData.Commands.Heartbeat;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Plugin.Heartbeat;

public class PlayerHeartbeatHandler : IRequestHandler<PlayersHeartbeatCommand>
{
    private readonly DataContext _context;
    private readonly IStatisticService _statisticService;

    public PlayerHeartbeatHandler(DataContext context, IStatisticService statisticService)
    {
        _context = context;
        _statisticService = statisticService;
    }

    public async Task Handle(PlayersHeartbeatCommand request, CancellationToken cancellationToken)
    {
        var identities = request.PlayerIdentities
            .ToArray(); // Performance optimization for PostgreSQL's "ANY" operator rather than list converting to "IN"
        var profiles = await _context.Players
            .AsTracking()
            .Where(p => identities.Contains(p.Identity))
            .ToListAsync(cancellationToken: cancellationToken);

        foreach (var profile in profiles)
        {
            profile.HeartBeat = DateTimeOffset.UtcNow;
            profile.PlayTime = profile.PlayTime.Add(new TimeSpan(0, 0, 4, 0));
            _context.Players.Update(profile);
        }

        await _statisticService.UpdateOnlineStatisticAsync(identities);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
