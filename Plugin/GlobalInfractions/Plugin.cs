using GlobalInfractions.Configuration;
using GlobalInfractions.Enums;
using GlobalInfractions.Managers;
using GlobalInfractions.Models;
using SharedLibraryCore;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions;

// Credit and inspiration: https://forums.mcbans.com/
// https://forums.mcbans.com/wiki/how-the-system-works/

public class Plugin : IPlugin
{
    private readonly IServiceProvider _serviceProvider;
    public const string PluginName = "Global Infractions";
    public string Name => PluginName;
    public float Version => 20221218f;
    public string Author => "Amos";
#if DEBUG
    public const bool FeaturesEnabled = true;
#else
    public const bool FeaturesEnabled = false;
#endif
    private static bool _pluginEnabled;
    public static bool InstanceActive { get; set; }
    public static EndpointManager EndpointManager = null!;
    public static InstanceDto Instance = null!;
    public static TranslationStrings Translations = null!;
    public static HeartBeatManager HeartBeatManager = null!;
    private readonly IConfigurationHandler<ConfigurationModel> _configurationHandler;
    
    public Plugin(IServiceProvider serviceProvider, IConfigurationHandlerFactory configurationHandlerFactory)
    {
        _serviceProvider = serviceProvider;
        _configurationHandler = configurationHandlerFactory.GetConfigurationHandler<ConfigurationModel>("GlobalInfractionsSettings");
    }

    public async Task OnEventAsync(GameEvent gameEvent, Server server)
    {
        if (!_pluginEnabled) return;

        switch (gameEvent.Type)
        {
            case GameEvent.EventType.Join:
                await EndpointManager.OnJoin(gameEvent.Origin);
                break;
            case GameEvent.EventType.Disconnect:
                EndpointManager.RemoveFromProfiles(gameEvent.Origin);
                break;
            case GameEvent.EventType.Warn:
                await EndpointManager.NewInfraction(InfractionType.Warn, gameEvent.Origin, gameEvent.Target, gameEvent.Data);
                break;
            case GameEvent.EventType.Kick:
                await EndpointManager.NewInfraction(InfractionType.Kick, gameEvent.Origin, gameEvent.Target, gameEvent.Data);
                break;
            case GameEvent.EventType.TempBan:
                await EndpointManager.NewInfraction(InfractionType.TempBan, gameEvent.Origin, gameEvent.Target, gameEvent.Data,
                    duration: (TimeSpan)gameEvent.Extra);
                break;
            case GameEvent.EventType.Ban:
                await EndpointManager.NewInfraction(InfractionType.Ban, gameEvent.Origin, gameEvent.Target, gameEvent.Data);
                break;
            case GameEvent.EventType.Unban:
                await EndpointManager.NewInfraction(InfractionType.Unban, gameEvent.Origin, gameEvent.Target, gameEvent.Data);
                break;
        }
    }

    public async Task OnLoadAsync(IManager manager)
    {
        #if DEBUG
        Console.WriteLine($"[{PluginName}] Loading... !! DEBUG MODE !!");
        #else
        Console.WriteLine($"[{PluginName}] Loading...");
        #endif

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

        var config = _configurationHandler.Configuration();
        HeartBeatManager = new HeartBeatManager(_serviceProvider, config);
        EndpointManager = new EndpointManager(_serviceProvider, config);
        
        Translations = config.Translations;

        // Update the instance and check its state
        Instance = new InstanceDto
        {
            InstanceGuid = Guid.Parse(manager.GetApplicationSettings().Configuration().Id),
            InstanceName = config.InstanceNameOverride ?? manager.GetApplicationSettings().Configuration().WebfrontCustomBranding,
            InstanceIp = await Utilities.GetExternalIP(),
            ApiKey = config.ApiKey,
            HeartBeat = DateTimeOffset.UtcNow
        };

        _pluginEnabled = await EndpointManager.UpdateInstance(Instance);
        
        // If successful reply, get active state and start heartbeat
        if (_pluginEnabled)
        {
            InstanceActive = await EndpointManager.IsInstanceActive(Instance);
            Instance.Active = InstanceActive;
            
            if (InstanceActive)
            {
                Console.WriteLine($"[{PluginName}] Activated.");
                Console.WriteLine($"[{PluginName}] Infractions and users will be reported to the API.");
            }
            else
            {
                Console.WriteLine($"[{PluginName}] Not activated. Read-only access.");
                Console.WriteLine($"[{PluginName}] To activate your access. Please visit https://discord.gg/Arruj6DWvp");
            }

            HeartBeatManager.HeartbeatTimer();
            Console.WriteLine($"[{PluginName}] Loaded successfully. Version: {Version}");
            return;
        }

        Console.WriteLine($"[{PluginName}] Failed to load. Is the API running?");
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
