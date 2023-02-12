using System.Timers;
using BanHub.Configuration;
using BanHub.Models;
using BanHub.Services;
using Timer = System.Timers.Timer;

namespace BanHub.Managers;

public class HeartBeatManager
{
    private readonly InstanceDto _instanceMeta;
    private readonly HeartBeatEndpoint _heartBeatEndpoint;

    public HeartBeatManager(InstanceDto instanceMeta, HeartBeatEndpoint heartBeatEndpoint)
    {
        _instanceMeta = instanceMeta;
        _heartBeatEndpoint = heartBeatEndpoint;
    }
    
    public async Task InstanceHeartBeat()
    {
        try
        {
            await _heartBeatEndpoint.PostInstanceHeartBeat(_instanceMeta);
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
            if (Plugin.EndpointManager.Profiles.Count is 0) return;
            var profileList = Plugin.EndpointManager.Profiles.Select(x => x.Value).ToList();
            await _heartBeatEndpoint.PostEntityHeartBeat(profileList);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}
