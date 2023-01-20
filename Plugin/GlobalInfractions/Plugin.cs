using Data.Models;
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
    public const string PluginName = "Global Infractions";
    public string Name => PluginName;
    public float Version => 20230108f;
    public string Author => "Amos";

    public static bool InstanceActive { get; set; }
    public static EndpointManager EndpointManager = null!;
    public static InstanceDto Instance = null!;
    public static TranslationStrings Translations = null!;
    public static IManager Manager = null!;
    public static List<int> WhitelistedClientIds = null!;
    private static HeartBeatManager _heartBeatManager = null!;
    private static bool _pluginEnabled;
    private readonly IConfigurationHandler<ConfigurationModel> _configurationHandler;
    private readonly IServiceProvider _serviceProvider;

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
            case GameEvent.EventType.Start:
                var serverDto = new ServerDto
                {
                    ServerId = $"{server.IP}:{server.Port}",
                    ServerName = server.Hostname.StripColors(),
                    ServerIp = server.IP,
                    ServerPort = server.Port,
                    Instance = Instance
                };
                await EndpointManager.OnStart(serverDto);
                break;
        }

        if (gameEvent.Origin is null || WhitelistedClientIds.Contains(gameEvent.Origin.ClientId)) return;
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

        var config = _configurationHandler.Configuration();

        if (!config.EnableGlobalInfractions)
        {
            _pluginEnabled = false;
            return;
        }

#if DEBUG
        Console.WriteLine($"[{PluginName}] Loading... !! DEBUG MODE !!");
#else
        Console.WriteLine($"[{PluginName}] Loading...");
#endif

        _heartBeatManager = new HeartBeatManager(_serviceProvider, config);
        EndpointManager = new EndpointManager(_serviceProvider, config);

        WhitelistedClientIds = config.WhitelistedClientIds.Distinct().ToList();
        Translations = config.Translations;

        // Update the instance and check its state
        Instance = new InstanceDto
        {
            InstanceGuid = Guid.Parse(manager.GetApplicationSettings().Configuration().Id),
            InstanceName = config.InstanceNameOverride ?? manager.GetApplicationSettings().Configuration().WebfrontCustomBranding,
            InstanceIp = await Utilities.GetExternalIP(),
            ApiKey = config.ApiKey,
            HeartBeat = DateTimeOffset.UtcNow,
            Servers = new List<ServerDto>()
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

            _heartBeatManager.HeartbeatTimer();
            Console.WriteLine($"[{PluginName}] Loaded successfully. Version: {Version}");
            return;
        }

        Console.WriteLine($"[{PluginName}] Failed to load. Is the API running?");
    }

    public async Task OnUnloadAsync()
    {
        if (!_pluginEnabled) return;

        _configurationHandler.Configuration().WhitelistedClientIds = WhitelistedClientIds;
        await _configurationHandler.Save();
        Console.WriteLine($"[{PluginName}] unloaded");
    }

    public Task OnTickAsync(Server server)
    {
        return Task.CompletedTask;
    }
}
