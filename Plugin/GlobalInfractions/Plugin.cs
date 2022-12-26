using System.Collections.Concurrent;
using GlobalInfractions.Configuration;
using GlobalInfractions.Enums;
using GlobalInfractions.Models;
using SharedLibraryCore;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions;

public class Plugin : IPlugin
{
    public const string PluginName = "Global Infractions";
    public string Name => PluginName;
    public float Version => 20221218f;
    public string Author => "Amos";


    private readonly IConfigurationHandler<ConfigurationModel> _configurationHandler;
    public static InfractionManager InfractionManager = null!;
    public static HeartbeatManager HeartbeatManager = null!;
    public static IManager Manager = null!;
    public static bool Active { get; set; }

    public Plugin(IServiceProvider serviceProvider, IConfigurationHandler<ConfigurationModel> configurationHandler)
    {
        HeartbeatManager = new HeartbeatManager(serviceProvider);
        InfractionManager = new InfractionManager(serviceProvider);
        _configurationHandler = configurationHandler;
    }

    public async Task OnEventAsync(GameEvent gameEvent, Server server)
    {
        switch (gameEvent.Type)
        {
            case GameEvent.EventType.Join:
                await InfractionManager.UpdateProfile(gameEvent.Origin);
                break;
            case GameEvent.EventType.Disconnect:
                InfractionManager.RemoveFromProfiles(gameEvent.Origin);
                break;
            case GameEvent.EventType.Warn:
                await InfractionManager.NewInfraction(InfractionType.Warn, gameEvent.Origin, gameEvent.Target, gameEvent.Data);
                break;
            case GameEvent.EventType.Kick:
                await InfractionManager.NewInfraction(InfractionType.Kick, gameEvent.Origin, gameEvent.Target, gameEvent.Data);
                break;
            case GameEvent.EventType.TempBan:
                await InfractionManager.NewInfraction(InfractionType.TempBan, gameEvent.Origin, gameEvent.Target, gameEvent.Data,
                    duration: (TimeSpan)gameEvent.Extra);
                break;
            case GameEvent.EventType.Ban:
                await InfractionManager.NewInfraction(InfractionType.Ban, gameEvent.Origin, gameEvent.Target, gameEvent.Data);
                break;
            case GameEvent.EventType.Unban:
                await InfractionManager.NewInfraction(InfractionType.Unban, gameEvent.Origin, gameEvent.Target, gameEvent.Data);
                break;
        }
    }

    public async Task OnLoadAsync(IManager manager)
    {
        Console.WriteLine($"[{PluginName}] Loading...");
        Manager = manager;

        // Build configuration
        await _configurationHandler.BuildAsync();
        if (_configurationHandler.Configuration() == null)
        {
            Console.WriteLine($"[{PluginName}] Configuration not found, creating.");
            _configurationHandler.Set(new ConfigurationModel());
            await _configurationHandler.Save();
            await _configurationHandler.BuildAsync();
        }
        else
        {
            await _configurationHandler.Save();
        }
        

        // Check activation status
        await InfractionManager.GetInstance();
        Active = await InfractionManager.UpdateInstance();

        if (!Active)
        {
            Console.WriteLine($"[{PluginName}] Not activated. Read-only access.");
            Console.WriteLine($"[{PluginName}] To activate your access. Please visit <DISCORD>");
        }
        else
        {
            Console.WriteLine($"[{PluginName}] Activated.");
            Console.WriteLine($"[{PluginName}] Infractions and users will be reported to the API.");
        }

        // Start the heartbeat
        HeartbeatManager.HeartbeatTimer();

        Console.WriteLine($"[{PluginName}] Loaded successfully. Version: {Version}");
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
