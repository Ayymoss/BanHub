using BanHub.Models;
using BanHub.Services;
using BanHubData.Commands.Heartbeat;

namespace BanHub.Managers;

public class HeartBeatManager
{
    private readonly InstanceSlim _instanceSlim;
    private readonly HeartbeatService _heartbeatService;
    private readonly EndpointManager _endpointManager;

    public HeartBeatManager(InstanceSlim instanceSlim, HeartbeatService heartbeatService, EndpointManager endpointManager)
    {
        _instanceSlim = instanceSlim;
        _heartbeatService = heartbeatService;
        _endpointManager = endpointManager;
    }
    
    public async Task InstanceHeartBeat()
    {
        try
        {
            await _heartbeatService.PostInstanceHeartBeat(new InstanceHeartbeatCommand
            {
                ApiKey = _instanceSlim.ApiKey,
                InstanceGuid = _instanceSlim.InstanceGuid
            });
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    public async Task ClientHeartBeat()
    {
        if (!Plugin.InstanceActive) return;
        try
        {
            if (_endpointManager.Profiles.Count is 0) return;
            var players = _endpointManager.Profiles.Select(x => x.Value).ToList();
            await _heartbeatService.PostEntityHeartBeat(new PlayersHeartbeatCommand
            {
                InstanceGuid = _instanceSlim.InstanceGuid,
                PlayerIdentities = players
            });
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}
