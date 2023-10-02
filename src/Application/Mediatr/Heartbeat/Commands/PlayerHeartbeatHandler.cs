using BanHub.Application.Mediatr.Services.Events.Statistics;
using BanHub.Domain.Enums;
using BanHub.Domain.Interfaces.Repositories;
using MediatR;

namespace BanHub.Application.Mediatr.Heartbeat.Commands;

public class PlayerHeartbeatHandler(IPublisher publisher, IPlayerRepository playerRepository)
    : IRequestHandler<PlayersHeartbeatCommand, SignalREnums.ReturnState>
{
    public async Task<SignalREnums.ReturnState> Handle(PlayersHeartbeatCommand request, CancellationToken cancellationToken)
    {
        // Performance optimization for PostgreSQL's "ANY" operator rather than list converting to "IN"
        var identities = request.PlayerIdentities.ToArray();
        var profiles = await playerRepository.GetPlayerRangeAsync(identities, cancellationToken);
        var updatedProfiles = profiles.Select(profile => profile.UpdateHeartbeat()).ToList();
        await playerRepository.UpdatePlayerRangeAsync(updatedProfiles, cancellationToken);

        await publisher.Publish(notification: new UpdateOnlineStatisticNotification {Identities = identities},
            cancellationToken: cancellationToken);

        return SignalREnums.ReturnState.Ok;
    }
}
