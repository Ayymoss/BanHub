using BanHub.Domain.Enums;
using BanHub.Domain.Interfaces.Repositories;
using MediatR;

namespace BanHub.Application.Mediatr.Heartbeat.Commands;

public class CommunityHeartbeatHandler(ICommunityRepository communityRepository)
    : IRequestHandler<CommunityHeartbeatCommand, SignalREnums.ReturnState>
{
    public async Task<SignalREnums.ReturnState> Handle(CommunityHeartbeatCommand request, CancellationToken cancellationToken)
    {
        var community = await communityRepository.GetCommunityAsync(request.CommunityGuid, cancellationToken);
        var isKeyValid = await communityRepository.IsCommunityApiKeyValidAsync(request.CommunityGuid, request.ApiKey, cancellationToken);

        if (community is null || !isKeyValid) return SignalREnums.ReturnState.NotFound;

        community.HeartBeat = DateTimeOffset.UtcNow;
        await communityRepository.AddOrUpdateCommunityAsync(community, cancellationToken);

        return SignalREnums.ReturnState.Ok;
    }
}
