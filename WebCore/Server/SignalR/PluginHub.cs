using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Services;
using BanHubData.Commands.Heartbeat;
using BanHubData.Enums;
using BanHubData.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace BanHub.WebCore.Server.SignalR;

public class PluginHub : Hub
{
    private readonly IMediator _mediator;
    private readonly IStatisticService _statisticService;
    private readonly IPluginAuthenticationCache _pluginAuthenticationCache;
    private readonly ICommunityConnectionManager _connectionManager;
    private readonly Configuration _config;

    public PluginHub(IMediator mediator, IStatisticService statisticService, IPluginAuthenticationCache pluginAuthenticationCache,
        ICommunityConnectionManager connectionManager, Configuration config)
    {
        _mediator = mediator;
        _statisticService = statisticService;
        _pluginAuthenticationCache = pluginAuthenticationCache;
        _connectionManager = connectionManager;
        _config = config;
    }

    public async Task<SignalREnums.ReturnState> CommunityHeartbeat(CommunityHeartbeatCommand request)
    {
        if (request.PluginVersion < _config.PluginVersion) return SignalREnums.ReturnState.VersionToOld;

        _connectionManager.AddOrUpdate(request.CommunityGuid, Context.ConnectionId);
        var result = await _mediator.Send(request);
        return result;
    }

    public async Task<SignalREnums.ReturnState> PlayersHeartbeat(PlayersHeartbeatCommand request)
    {
        var result = await _mediator.Send(request);
        return result;
    }

    public async Task<SignalREnums.ReturnState> PlayerJoined(PlayerJoined request)
    {
        if (request.PluginVersion < _config.PluginVersion) return SignalREnums.ReturnState.VersionToOld;
        if (!_pluginAuthenticationCache.ExistsApiKey(request.CommunityApiKey)) return SignalREnums.ReturnState.NotActivated;
        await _statisticService.UpdateOnlineStatisticAsync(new[] {request.Identity});
        return SignalREnums.ReturnState.Ok;
    }
}
