using BanHub.Models;
using BanHub.Services;
using BanHubData.Commands.Heartbeat;

namespace BanHub.Managers;

public class HeartbeatManager
{
    private readonly CommunitySlim _communitySlim;
    private readonly HeartbeatService _heartbeatService;
    private readonly EndpointManager _endpointManager;

    public HeartbeatManager(CommunitySlim communitySlim, HeartbeatService heartbeatService, EndpointManager endpointManager)
    {
        _communitySlim = communitySlim;
        _heartbeatService = heartbeatService;
        _endpointManager = endpointManager;
    }
    
    public async Task CommunityHeartbeat(string version)
    {
        try
        {
            await _heartbeatService.PostCommunityHeartbeat(new CommunityHeartbeatCommand
            {
                PluginVersion = new Version(version),
                ApiKey = _communitySlim.ApiKey,
                CommunityGuid = _communitySlim.CommunityGuid
            });
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    public async Task ClientHeartbeat(string version)
    {
        if (!_communitySlim.Active) return;
        try
        {
            if (_endpointManager.Profiles.Count is 0) return;
            var players = _endpointManager.Profiles.Select(x => x.Value).ToList();
            await _heartbeatService.PostEntityHeartbeat(new PlayersHeartbeatCommand
            {
                PluginVersion = new Version(version),
                CommunityGuid = _communitySlim.CommunityGuid,
                PlayerIdentities = players
            });
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}
