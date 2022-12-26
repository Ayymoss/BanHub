using System.Net;
using System.Net.Http.Json;
using System.Timers;
using GlobalInfractions.Services;
using Timer = System.Timers.Timer;

namespace GlobalInfractions;

public class HeartbeatManager
{
    private readonly HeartBeatEndpoint _heartBeatEndpoint;

    public HeartbeatManager(IServiceProvider serviceProvider)
    {
        _heartBeatEndpoint = new HeartBeatEndpoint(serviceProvider);
    }

    public void HeartbeatTimer()
    {
        var timer = new Timer();
        timer.Interval = 30000;
        timer.AutoReset = true;
        timer.Elapsed += InstanceHeartbeat;
        timer.Elapsed += ClientHeartbeat;
        timer.Enabled = true;
    }

    private async void InstanceHeartbeat(object? sender, ElapsedEventArgs e)
    {
        var instance = Plugin.InfractionManager.Instance;
        await _heartBeatEndpoint.PostInstanceHeartBeat(instance);
    }

    private async void ClientHeartbeat(object? sender, ElapsedEventArgs e)
    {
        if (Plugin.InfractionManager.Profiles.Count is 0) return;
        var profileList = Plugin.InfractionManager.Profiles.Select(x => x.Value).ToList();
        await _heartBeatEndpoint.PostEntityHeartBeat(profileList);
    }
}
