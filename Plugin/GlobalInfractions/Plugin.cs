using GlobalInfractions.Configuration;
using GlobalInfractions.Enums;
using GlobalInfractions.Managers;
using GlobalInfractions.Models;
using SharedLibraryCore;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions;

public class Plugin : IPlugin
{
    public const string PluginName = "Global Infractions";
    public string Name => PluginName;
    public float Version => 20221218f;
    public string Author => "Amos";

    public static bool Active { get; set; }
    public static EndpointManager EndpointManager = null!;
    public static InstanceDto Instance = null!;
    public static TranslationStrings Translations = null!;
    private readonly IConfigurationHandler<ConfigurationModel> _configurationHandler;
    private readonly HeartbeatManager _heartbeatManager;
    
    public Plugin(IServiceProvider serviceProvider, IConfigurationHandlerFactory configurationHandlerFactory)
    {
        _heartbeatManager = new HeartbeatManager(serviceProvider);
        EndpointManager = new EndpointManager(serviceProvider);
        _configurationHandler = configurationHandlerFactory.GetConfigurationHandler<ConfigurationModel>("ReservedClientsSettings");
    }

    public async Task OnEventAsync(GameEvent gameEvent, Server server)
    {
        switch (gameEvent.Type)
        {
            case GameEvent.EventType.Join:
                await EndpointManager.OnJoin(gameEvent.Origin, Instance);
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
        Console.WriteLine($"[{PluginName}] Loading...");

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

        Translations = _configurationHandler.Configuration().Translations;

        // Update the instance and check its state
        Instance = new InstanceDto
        {
            InstanceGuid = Guid.Parse(manager.GetApplicationSettings().Configuration().Id),
            InstanceName = manager.GetApplicationSettings().Configuration().WebfrontCustomBranding,
            InstanceIp = await Utilities.GetExternalIP(),
            ApiKey = _configurationHandler.Configuration().ApiKey,
            Heartbeat = DateTimeOffset.UtcNow
        };

        await EndpointManager.UpdateInstance(Instance);
        Active = await EndpointManager.IsInstanceActive(Instance);

        if (!Active)
        {
            Console.WriteLine($"[{PluginName}] Not activated. Read-only access.");
            Console.WriteLine($"[{PluginName}] To activate your access. Please visit https://discord.gg/Arruj6DWvp");
        }
        else
        {
            Console.WriteLine($"[{PluginName}] Activated.");
            Console.WriteLine($"[{PluginName}] Infractions and users will be reported to the API.");
        }

        // Start the heartbeat timer
        _heartbeatManager.HeartbeatTimer();

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
