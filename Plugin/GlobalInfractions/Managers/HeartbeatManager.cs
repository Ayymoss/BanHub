using System.Timers;
using GlobalInfractions.Services;
using Timer = System.Timers.Timer;

namespace GlobalInfractions.Managers;

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
        try
        {
            var instance = Plugin.Instance;
            await _heartBeatEndpoint.PostInstanceHeartBeat(instance);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    private async void ClientHeartbeat(object? sender, ElapsedEventArgs e)
    {
        if (!Plugin.Active) return;
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
