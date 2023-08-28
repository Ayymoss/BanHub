using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Notifications.Statistics;
using BanHub.WebCore.Server.Services;
using BanHubData.Commands.Heartbeat;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Requests.Plugin.Heartbeat;

public class PlayerHeartbeatHandler : IRequestHandler<PlayersHeartbeatCommand, SignalREnums.ReturnState>
{
    private readonly DataContext _context;
    private readonly IMediator _mediator;
    private readonly Configuration _config;

    public PlayerHeartbeatHandler(DataContext context, IMediator mediator, Configuration config)
    {
        _context = context;
        _mediator = mediator;
        _config = config;
    }

    public async Task<SignalREnums.ReturnState> Handle(PlayersHeartbeatCommand request, CancellationToken cancellationToken)
    {
        if (request.PluginVersion < _config.PluginVersion) return SignalREnums.ReturnState.VersionToOld;

        var identities = request.PlayerIdentities
            .ToArray(); // Performance optimization for PostgreSQL's "ANY" operator rather than list converting to "IN"
        var profiles = await _context.Players
            .AsTracking()
            .Where(p => identities.Contains(p.Identity))
            .ToListAsync(cancellationToken: cancellationToken);

        foreach (var profile in profiles)
        {
            profile.Heartbeat = DateTimeOffset.UtcNow;
            profile.PlayTime = profile.PlayTime.Add(new TimeSpan(0, 0, 4, 0));
            _context.Players.Update(profile);
        }

        await _mediator.Publish(notification: new UpdateOnlineStatisticNotification {Identities = identities},
            cancellationToken: cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return SignalREnums.ReturnState.Ok;
    }
}
