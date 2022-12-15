using System.Net;
using System.Net.Http.Json;
using System.Text;
using GlobalBans.Models;
using SharedLibraryCore;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace GlobalBans;

public class Plugin : IPlugin
{
    public const string PluginName = "Global Bans";
    public string Name => PluginName;
    public float Version => 20221213f;
    public string Author => "Amos";

    public bool Enabled { get; set; }

    public async Task OnEventAsync(GameEvent gameEvent, Server server)
    {
        if (!Enabled) return;

        switch (gameEvent.Type)
        {
            case GameEvent.EventType.Join:
                await UpdateClient(gameEvent.Origin);
                break;
            case GameEvent.EventType.Disconnect:
                break;
            case GameEvent.EventType.Warn:
                break;
            case GameEvent.EventType.WarnClear:
                break;
            case GameEvent.EventType.Kick:
                break;
            case GameEvent.EventType.TempBan:
                break;
            case GameEvent.EventType.Ban:
                break;
            case GameEvent.EventType.Unban:
                break;
        }
    }

    private async Task UpdateClient(EFClient client)
    {
        var httpClient = new HttpClient();

        var userRequest = new ProfileRequestModel
        {
            ProfileGuid = client.GuidString,
            ProfileGame = client.GameName.ToString(),
            ProfileIdentifier =
                Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Concat(client.GuidString, client.GameName.ToString()))),
            ProfileMeta = new ProfileMetaRequestModel
            {
                UserName = client.CleanedName,
                IpAddress = client.IPAddressString
            }
        };
        var postResponse = await httpClient.PostAsJsonAsync("http://localhost:5001/api/Profile", userRequest);
        Console.WriteLine($"[{PluginName}] {client.CleanedName} : {postResponse.StatusCode} - {postResponse.ReasonPhrase}");
    }

    private async Task<bool> UpdateInstance(IManager manager)
    {
        var instanceGuid = manager.GetApplicationSettings().Configuration().Id;
        var instanceName = manager.GetApplicationSettings().Configuration().WebfrontCustomBranding;
        var instanceIp = await Utilities.GetExternalIP();
        var apiKey = Guid.Parse("92B3064E-F51D-49F7-9337-C409669B7FDC"); // TODO: Change this.

        Console.WriteLine($"[{PluginName}] Updating instance {instanceName} ({instanceGuid}) - {instanceIp}");

        var model = new InstanceRequestModel
        {
            InstanceGuid = Guid.Parse(instanceGuid),
            InstanceName = instanceName,
            InstanceIp = instanceIp,
            ApiKey = apiKey
        };

        var httpClient = new HttpClient();
        var postServerResponse = await httpClient.PostAsJsonAsync("http://localhost:5000/api/Instance", model);
        var enabled = false;

        switch (postServerResponse.StatusCode)
        {
            case HttpStatusCode.Accepted:
                enabled = true;
                break;
            case HttpStatusCode.OK:
                enabled = false;
                break;
        }

        if (!enabled)
        {
            Console.WriteLine($"[{PluginName}] Global Bans plugin is disabled");
            Console.WriteLine($"[{PluginName}] To activate your access. Please visit <DISCORD>");
        }
        else
        {
            Console.WriteLine($"[{PluginName}] Global Bans plugin is enabled");
            Console.WriteLine($"[{PluginName}] Infractions will be reported to the Global Bans list.");
        }

        return enabled;
    }

    public async Task OnLoadAsync(IManager manager)
    {
        Console.WriteLine($"[{PluginName}] Global Bans plugin started");
        Enabled = await UpdateInstance(manager);
        Console.WriteLine($"[{PluginName}] Global Bans plugin loaded");
    }

    public Task OnUnloadAsync()
    {
        return Task.CompletedTask;
    }

    public Task OnTickAsync(Server server)
    {
        return Task.CompletedTask;
    }
}
