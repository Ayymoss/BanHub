using BanHub.Configuration;
using BanHub.Models;
using BanHub.Services;
using BanHubData.Commands.Heartbeat;

namespace BanHub.Managers;

public class HeartBeatManager
{
    private readonly CommunitySlim _communitySlim;
    private readonly HeartbeatService _heartbeatService;
    private readonly EndpointManager _endpointManager;

    public HeartBeatManager(CommunitySlim communitySlim, HeartbeatService heartbeatService, EndpointManager endpointManager)
    {
        _communitySlim = communitySlim;
        _heartbeatService = heartbeatService;
        _endpointManager = endpointManager;
    }
    
    public async Task CommunityHeartbeat()
    {
        try
        {
            await _heartbeatService.PostCommunityHeartbeat(new CommunityHeartbeatCommand
            {
                ApiKey = _communitySlim.ApiKey,
                CommunityGuid = _communitySlim.CommunityGuid
            });
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    public async Task ClientHeartbeat()
    {
        if (!Plugin.CommunityActive) return;
        try
        {
            if (_endpointManager.Profiles.Count is 0) return;
            var players = _endpointManager.Profiles.Select(x => x.Value).ToList();
            Console.WriteLine($"[{BanHubConfiguration.Name}] Sending heartbeat for {players.Count} players");
            await _heartbeatService.PostEntityHeartbeat(new PlayersHeartbeatCommand
            {
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
