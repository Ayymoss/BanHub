using BanHub.Application.Mediatr.Heartbeat.Commands;
using BanHub.Application.Mediatr.Services.Events.Statistics;
using BanHub.Application.Utilities;
using BanHub.Domain.Enums;
using BanHub.Domain.Interfaces;
using BanHub.Domain.Interfaces.Services;
using BanHub.Domain.Interfaces.SignalR;
using BanHub.Domain.ValueObjects.Plugin;
using BanHub.Domain.ValueObjects.Plugin.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace BanHub.Infrastructure.SignalR.Hubs;

public class PluginHub(IMediator mediator, IPluginAuthenticationCache pluginAuthenticationCache,
        ICommunityConnectionManager connectionManager, Configuration config)
    : Hub, IPluginHub
{
    public async Task<SignalREnums.ReturnState> CommunityHeartbeat(CommunityHeartbeatCommandSlim request)
    {
        var convertedRequest = new CommunityHeartbeatCommand
        {
            ApiKey = request.ApiKey,
            CommunityGuid = request.CommunityGuid
        };

        connectionManager.AddOrUpdate(convertedRequest.CommunityGuid, Context.ConnectionId);
        var result = await mediator.Send(convertedRequest);
        return result;
    }

    public async Task<SignalREnums.ReturnState> PlayersHeartbeat(PlayersHeartbeatCommandSlim request)
    {
        var convertedRequest = new PlayersHeartbeatCommand
        {
            CommunityGuid = request.CommunityGuid,
            PlayerIdentities = request.PlayerIdentities
        };
        var result = await mediator.Send(convertedRequest);
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
