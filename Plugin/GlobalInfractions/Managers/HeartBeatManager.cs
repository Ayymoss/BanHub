using System.Timers;
using GlobalInfractions.Configuration;
using GlobalInfractions.Models;
using GlobalInfractions.Services;
using Timer = System.Timers.Timer;

namespace GlobalInfractions.Managers;

public class HeartBeatManager
{
    private readonly ConfigurationModel _configurationModel;
    private readonly HeartBeatEndpoint _heartBeatEndpoint;

    public HeartBeatManager(IServiceProvider serviceProvider, ConfigurationModel configurationModel)
    {
        _configurationModel = configurationModel;
        _heartBeatEndpoint = new HeartBeatEndpoint(configurationModel);
    }

    public void HeartbeatTimer()
    {
        var timer = new Timer();
        timer.Interval = 30000;
        timer.AutoReset = true;
        timer.Elapsed += InstanceHeartBeat;
        timer.Elapsed += ClientHeartBeat;
        timer.Enabled = true;
    }

    private async void InstanceHeartBeat(object? sender, ElapsedEventArgs e)
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

    private async void ClientHeartBeat(object? sender, ElapsedEventArgs e)
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
