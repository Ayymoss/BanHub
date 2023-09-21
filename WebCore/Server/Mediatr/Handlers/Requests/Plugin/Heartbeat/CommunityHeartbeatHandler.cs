using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Services;
using BanHubData.Enums;
using BanHubData.Mediatr.Commands.Requests.Heartbeat;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Plugin.Heartbeat;

public class CommunityHeartbeatHandler(DataContext context) : IRequestHandler<CommunityHeartbeatCommand, SignalREnums.ReturnState>
{
    public async Task<SignalREnums.ReturnState> Handle(CommunityHeartbeatCommand request, CancellationToken cancellationToken)
    {
        var instance = await context.Communities
            .AsTracking()
            .FirstOrDefaultAsync(x => x.CommunityGuid == request.CommunityGuid && x.ApiKey == request.ApiKey,
                cancellationToken: cancellationToken);
        if (instance is null) return SignalREnums.ReturnState.NotFound;

        instance.HeartBeat = DateTimeOffset.UtcNow;
        context.Communities.Update(instance);
        await context.SaveChangesAsync(cancellationToken);
        return SignalREnums.ReturnState.Ok;
    }
}
