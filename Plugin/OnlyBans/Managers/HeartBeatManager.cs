using BanHub.Services;
using Data.Commands.Heartbeat;
using Data.Domains;

namespace BanHub.Managers;

public class HeartBeatManager
{
    private readonly Instance _instanceMeta;
    private readonly HeartBeatEndpoint _heartBeatEndpoint;
    private readonly EndpointManager _endpointManager;

    public HeartBeatManager(Instance instanceMeta, HeartBeatEndpoint heartBeatEndpoint, EndpointManager endpointManager)
    {
        _instanceMeta = instanceMeta;
        _heartBeatEndpoint = heartBeatEndpoint;
        _endpointManager = endpointManager;
    }
    
    public async Task InstanceHeartBeat()
    {
        try
        {
            await _heartBeatEndpoint.PostInstanceHeartBeat(new InstanceHeartbeatCommand
            {
                ApiKey = _instanceMeta.ApiKey,
                InstanceGuid = _instanceMeta.InstanceGuid
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
            await _heartBeatEndpoint.PostEntityHeartBeat(new PlayersHeartbeartCommand
            {
                InstanceGuid = _instanceMeta.InstanceGuid,
                PlayerIdentities = players
            });
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}
