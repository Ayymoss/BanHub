using System.Net;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Timers;
using GlobalInfractions.Models;
using Timer = System.Timers.Timer;

namespace GlobalInfractions;

public class HeartbeatManager
{
    private static readonly HttpClient HttpClient = new();

    public void HeartbeatTimer()
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
        try
        {
            var instance = Plugin.InfractionManager.Instance;
            var response = await HttpClient.PostAsJsonAsync("http://localhost:5000/api/Heartbeat/Instance", instance);

            if (response.StatusCode is not HttpStatusCode.OK) return;

            var replyString = await response.Content.ReadAsStringAsync();
            var boolParse = bool.TryParse(replyString, out var boolOut);
            if (boolParse) Plugin.Active = boolOut;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    private static async void ClientHeartbeat(object? sender, ElapsedEventArgs e)
    {
        try
        {
            if (Plugin.InfractionManager.Profiles.Count is 0) return;
            var profileList = Plugin.InfractionManager.Profiles.Select(x => x.Value).ToList();
            var response = await HttpClient.PostAsJsonAsync("http://localhost:5000/api/Heartbeat/Profiles", profileList);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}
