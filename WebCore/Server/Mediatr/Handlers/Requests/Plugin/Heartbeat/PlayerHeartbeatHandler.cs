using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Mediatr.Commands.Events.Services.Statistics;
using BanHub.WebCore.Server.Utilities;
using BanHubData.Enums;
using BanHubData.Mediatr.Commands.Requests.Heartbeat;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Plugin.Heartbeat;

public class PlayerHeartbeatHandler(DataContext context, IPublisher publisher) : IRequestHandler<PlayersHeartbeatCommand, SignalREnums.ReturnState>
{

    public async Task<SignalREnums.ReturnState> Handle(PlayersHeartbeatCommand request, CancellationToken cancellationToken)
    {
        var identities = request.PlayerIdentities
            .ToArray(); // Performance optimization for PostgreSQL's "ANY" operator rather than list converting to "IN"
        var profiles = await context.Players
            .AsTracking()
            .Where(p => identities.Contains(p.Identity))
            .ToListAsync(cancellationToken: cancellationToken);

        foreach (var profile in profiles)
        {
            profile.Heartbeat = DateTimeOffset.UtcNow;
            profile.PlayTime = profile.PlayTime.Add(new TimeSpan(0, 0, 4, 0));
            context.Players.Update(profile);
        }

        await publisher.Publish(notification: new UpdateOnlineStatisticNotification {Identities = identities},
            cancellationToken: cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return SignalREnums.ReturnState.Ok;
    }
}
