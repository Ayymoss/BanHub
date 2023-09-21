using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Mediatr.Commands.Events.Services.Statistics;
using BanHub.WebCore.Server.Utilities;
using BanHubData.Enums;
using BanHubData.Mediatr.Commands.Requests.Heartbeat;
using BanHubData.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace BanHub.WebCore.Server.SignalR;

public class PluginHub(IMediator mediator, IPluginAuthenticationCache pluginAuthenticationCache,
        ICommunityConnectionManager connectionManager, Configuration config)
    : Hub
{
    public async Task<SignalREnums.ReturnState> CommunityHeartbeat(CommunityHeartbeatCommand request)
    {
        connectionManager.AddOrUpdate(request.CommunityGuid, Context.ConnectionId);
        var result = await mediator.Send(request);
        return result;
    }

    public async Task<SignalREnums.ReturnState> PlayersHeartbeat(PlayersHeartbeatCommand request)
    {
        var result = await mediator.Send(request);
        return result;
    }

    public async Task<SignalREnums.ReturnState> PlayerJoined(PlayerJoined request)
    {
        if (request.PluginVersion < config.PluginVersion) return SignalREnums.ReturnState.VersionToOld;
        if (!pluginAuthenticationCache.ExistsApiKey(request.CommunityApiKey)) return SignalREnums.ReturnState.NotActivated;
        await mediator.Publish(new UpdateOnlineStatisticNotification {Identities = new[] {request.Identity}});
        await mediator.Publish(new UpdatePlayerServerStatisticNotification {Identity = request.Identity, ServerId = request.ServerId});
        return SignalREnums.ReturnState.Ok;
    }
}
