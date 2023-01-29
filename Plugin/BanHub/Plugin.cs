using BanHub.Configuration;
using BanHub.Enums;
using BanHub.Managers;
using BanHub.Models;
using BanHub.Services;
using Microsoft.Extensions.DependencyInjection;
using SharedLibraryCore;
using SharedLibraryCore.Interfaces;

namespace BanHub;

// Credit and inspiration: https://forums.mcbans.com/
// https://forums.mcbans.com/wiki/how-the-system-works/

public class Plugin : IPlugin
{
    public const string PluginName = "Ban Hub";
    public string Name => PluginName;
    public float Version => 20230108f;
    public string Author => "Amos";

    public static bool InstanceActive { get; set; }
    public static EndpointManager EndpointManager { get; set; } = null!;
    private InstanceDto InstanceMeta { get; set; }
    public static TranslationStrings Translations { get; set; } = null!;
    public static List<int> WhitelistedClientIds { get; set; } = null!;
    private readonly HeartBeatManager _heartBeatManager;
    private static bool _pluginEnabled;
    private readonly IConfigurationHandler<ConfigurationModel> _configurationHandler;

    public Plugin(IConfigurationHandlerFactory configurationHandlerFactory, InstanceDto instance, HeartBeatManager heartBeatManager, EndpointManager endpointManager)
    {
        _heartBeatManager = heartBeatManager;
        EndpointManager = endpointManager;
        InstanceMeta = instance;
        _configurationHandler = configurationHandlerFactory.GetConfigurationHandler<ConfigurationModel>("BanHubSettings");
    }

    public static void RegisterDependencies(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton(sp =>
        {
            var configHandler = sp.GetRequiredService<IConfigurationHandler<ConfigurationModel>>();
            configHandler.BuildAsync().GetAwaiter().GetResult();
            return new HeartBeatEndpoint(configHandler.Configuration());
        });
        serviceCollection.AddSingleton(sp =>
        {
            var configHandler = sp.GetRequiredService<IConfigurationHandler<ConfigurationModel>>();
            configHandler.BuildAsync().GetAwaiter().GetResult();
            return new EntityEndpoint(configHandler.Configuration());
        });
        serviceCollection.AddSingleton(sp =>
        {
            var configHandler = sp.GetRequiredService<IConfigurationHandler<ConfigurationModel>>();
            configHandler.BuildAsync().GetAwaiter().GetResult();
            return new InfractionEndpoint(configHandler.Configuration());
        });
        serviceCollection.AddSingleton(sp =>
        {
            var configHandler = sp.GetRequiredService<IConfigurationHandler<ConfigurationModel>>();
            configHandler.BuildAsync().GetAwaiter().GetResult();
            return new InstanceEndpoint(configHandler.Configuration());
        });
        serviceCollection.AddSingleton(sp =>
        {
            var configHandler = sp.GetRequiredService<IConfigurationHandler<ConfigurationModel>>();
            configHandler.BuildAsync().GetAwaiter().GetResult();
            return new ServerEndpoint(configHandler.Configuration());
        });
        serviceCollection.AddSingleton(new InstanceDto());
        serviceCollection.AddSingleton<HeartBeatManager>();
        serviceCollection.AddSingleton<EndpointManager>();
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
                    Instance = InstanceMeta
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

     

        WhitelistedClientIds = config.WhitelistedClientIds.Distinct().ToList();
        Translations = config.Translations;

        // Update the instance and check its state
        InstanceMeta.InstanceGuid = Guid.Parse(manager.GetApplicationSettings().Configuration().Id);
        InstanceMeta.InstanceName = config.InstanceNameOverride ?? manager.GetApplicationSettings().Configuration().WebfrontCustomBranding;
        InstanceMeta.InstanceIp = manager.ExternalIPAddress;
        InstanceMeta.ApiKey = config.ApiKey;
        InstanceMeta.HeartBeat = DateTimeOffset.UtcNow;

        _pluginEnabled = await EndpointManager.UpdateInstance(InstanceMeta);

        // If successful reply, get active state and start heartbeat
        if (_pluginEnabled)
        {
            InstanceActive = await EndpointManager.IsInstanceActive(InstanceMeta);
            InstanceMeta.Active = InstanceActive;

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
