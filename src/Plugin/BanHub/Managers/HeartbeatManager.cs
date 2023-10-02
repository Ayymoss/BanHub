using BanHub.Domain.ValueObjects.Plugin.SignalR;
using BanHub.Plugin.Models;
using BanHub.Plugin.SignalR;
using Microsoft.Extensions.Logging;

namespace BanHub.Plugin.Managers;

public class HeartbeatManager
{
    private readonly CommunitySlim _communitySlim;
    private readonly PluginHub _pluginHub;
    private readonly EndpointManager _endpointManager;
    private readonly ILogger<HeartbeatManager> _logger;

    public HeartbeatManager(CommunitySlim communitySlim, PluginHub pluginHub, EndpointManager endpointManager,
        ILogger<HeartbeatManager> logger)
    {
        _communitySlim = communitySlim;
        _pluginHub = pluginHub;
        _endpointManager = endpointManager;
        _logger = logger;
    }

    public async Task CommunityHeartbeatAsync()
    {
        await _pluginHub.HeartbeatAsync(new CommunityHeartbeatCommandSlim
        {
            ApiKey = _communitySlim.ApiKey,
            CommunityGuid = _communitySlim.CommunityGuid
        });
        _logger.LogDebug("Community heartbeat sent");
    }

    public async Task ClientHeartbeatAsync()
    {
        if (!_communitySlim.Active) return;

        if (_endpointManager.Profiles.Count is 0) return;
        var players = _endpointManager.Profiles.Select(x => x.Value).ToList();
        await _pluginHub.HeartbeatAsync(new PlayersHeartbeatCommandSlim
        {
            CommunityGuid = _communitySlim.CommunityGuid,
            PlayerIdentities = players
        });
        _logger.LogDebug("Client heartbeat sent");
    }

    public async Task PlayerJoinedAsync(string identity, string version)
    {
        if (!_communitySlim.Active) return;

        await _pluginHub.HeartbeatAsync(new PlayerJoined
        { 
            PluginVersion = new Version(version),
            CommunityApiKey = _communitySlim.ApiKey,
            Identity = identity
        });
        _logger.LogDebug("Player joined heartbeat sent");
    }
}
