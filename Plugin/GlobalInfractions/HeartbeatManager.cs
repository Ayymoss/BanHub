using System.Net.Http.Json;
using System.Timers;
using GlobalInfractions.Models;
using SharedLibraryCore.Database.Models;
using SQLitePCL;
using Timer = System.Timers.Timer;

namespace GlobalInfractions;

public class HeartbeatManager
{
    public static HttpClient HttpClient = new();

    public static void HeartbeatTimer()
    {
        var timer = new Timer();
        timer.Interval = 30000;
        timer.AutoReset = true;
        timer.Elapsed += InstanceHeartbeat;
        timer.Elapsed += ClientHeartbeat;
        timer.Enabled = true;
    }

    private static async void InstanceHeartbeat(object? sender, ElapsedEventArgs e)
    {
        var instance = Plugin.InfractionManager.GetInstance();
        await HttpClient.PostAsJsonAsync("http://localhost:5000/api/Heartbeat/Instance", instance);
    }

    private static async void ClientHeartbeat(object? sender, ElapsedEventArgs e)
    {
        var profileList = Plugin.InfractionManager.Profiles.Select(x => x.Value).ToList();

        await HttpClient.PostAsJsonAsync( "http://localhost:5000/api/Heartbeat/Profile", profileList);
    }

    
}
