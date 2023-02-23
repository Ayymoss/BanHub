using BanHub.Configuration;
using BanHub.Enums;
using BanHub.Managers;
using BanHub.Models;
using BanHub.Services;
using Microsoft.Extensions.DependencyInjection;
using SharedLibraryCore;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Events.Management;
using SharedLibraryCore.Events.Server;
using SharedLibraryCore.Interfaces;
using SharedLibraryCore.Interfaces.Events;

namespace BanHub;

public class Plugin : IPluginV2
{
    public string Name => "Ban Hub";
    public string Version => Utilities.GetVersionAsString();
    public string Author => "Amos";

    private InstanceDto Instance { get; }
    private IManager Manager { get; set; }
    public static bool InstanceActive { get; set; }
    public static EndpointManager EndpointManager { get; set; } = null!;
    public static TranslationStrings Translations { get; set; } = null!;
    private readonly HeartBeatManager _heartBeatManager;
    private readonly ConfigurationModel _config;
    private readonly ApplicationConfiguration _appConfig;
    private readonly WhitelistManager _whitelistManager;
    private static bool _pluginEnabled;

    public Plugin(InstanceDto instance, HeartBeatManager heartBeatManager, EndpointManager endpointManager, ConfigurationModel config,
        ApplicationConfiguration appConfig, WhitelistManager whitelistManager)
    {
        if (!config.EnableBanHub) return; // disable if not enabled in config

        _config = config;
        _appConfig = appConfig;
        _whitelistManager = whitelistManager;
        _heartBeatManager = heartBeatManager;
        EndpointManager = endpointManager;
        Instance = instance;

        IGameServerEventSubscriptions.MonitoringStarted += OnMonitoringStarted;
        IManagementEventSubscriptions.Load += OnLoad;
        IManagementEventSubscriptions.ClientStateAuthorized += OnClientStateAuthorized;
        IManagementEventSubscriptions.ClientStateDisposed += OnClientStateDisposed;
        IManagementEventSubscriptions.ClientPenaltyAdministered += OnClientPenaltyAdministered;
        IManagementEventSubscriptions.ClientPenaltyRevoked += OnClientPenaltyRevoked;
        IManagementEventSubscriptions.NotifyAfterDelayCompleted += OnNotifyAfterDelayCompleted;
    }

    public static void RegisterDependencies(IServiceCollection serviceCollection)
    {
        serviceCollection.AddConfiguration("BanHubSettings", new ConfigurationModel());

        serviceCollection.AddSingleton<HeartBeatEndpoint>();
        serviceCollection.AddSingleton<EntityEndpoint>();
        serviceCollection.AddSingleton<PenaltyEndpoint>();
        serviceCollection.AddSingleton<InstanceEndpoint>();
        serviceCollection.AddSingleton<ServerEndpoint>();
        serviceCollection.AddSingleton(new InstanceDto());
        serviceCollection.AddSingleton<HeartBeatManager>();
        serviceCollection.AddSingleton<EndpointManager>();
        serviceCollection.AddSingleton<WhitelistManager>();
    }

    private async Task OnMonitoringStarted(MonitorStartEvent startEvent, CancellationToken token)
    {
        var serverDto = new ServerDto
        {
            ServerId = startEvent.Server.Id,
            ServerName = startEvent.Server.ServerName.StripColors(),
            ServerIp = startEvent.Server.ListenAddress,
            ServerPort = startEvent.Server.ListenPort,
            ServerGame = Enum.Parse<Game>(startEvent.Server.GameCode.ToString()),
            Instance = Instance
        };
        await EndpointManager.OnStart(serverDto);
    }

    private async Task OnClientPenaltyRevoked(ClientPenaltyRevokeEvent penaltyEvent, CancellationToken arg2)
    {
        if (await _whitelistManager.IsWhitelisted(penaltyEvent.Client.ToPartialClient())) return;

        await EndpointManager.NewPenalty(penaltyEvent.Penalty.Type.ToString(),
            penaltyEvent.Penalty.Punisher.ToPartialClient(),
            penaltyEvent.Penalty.Offender.ToPartialClient(),
            penaltyEvent.Penalty.Offense);
    }

    private async Task OnClientPenaltyAdministered(ClientPenaltyEvent penaltyEvent, CancellationToken arg2)
    {
        if (await _whitelistManager.IsWhitelisted(penaltyEvent.Client.ToPartialClient())) return;

        await EndpointManager.NewPenalty(penaltyEvent.Penalty.Type.ToString(),
            penaltyEvent.Penalty.Punisher.ToPartialClient(),
            penaltyEvent.Penalty.Offender.ToPartialClient(),
            penaltyEvent.Penalty.Offense,
            duration: penaltyEvent.Penalty.Expires - DateTimeOffset.UtcNow);
    }

    private Task OnClientStateDisposed(ClientStateEvent clientEvent, CancellationToken arg2)
    {
        EndpointManager.RemoveFromProfiles(clientEvent.Client);
        return Task.CompletedTask;
    }

    private async Task OnClientStateAuthorized(ClientStateAuthorizeEvent clientEvent, CancellationToken arg2)
    {
        if (await _whitelistManager.IsWhitelisted(clientEvent.Client.ToPartialClient())) return;
        await EndpointManager.OnJoin(clientEvent.Client);
    }

    private async Task OnLoad(IManager manager, CancellationToken arg2)
    {
#if DEBUG
        Console.WriteLine($"[{ConfigurationModel.Name}] Loading... !! DEBUG MODE !!");
#else
        Console.WriteLine($"[{ConfigurationModel.Name}] Loading...");
#endif

        Manager = manager;
        Translations = _config.Translations;

        // Update the instance and check its state (Singleton)
        Instance.InstanceGuid = Guid.Parse(_appConfig.Id);
        Instance.InstanceIp = manager.ExternalIPAddress;
        Instance.ApiKey = _config.ApiKey;

        // We need a copy of this since we don't really want the other values being sent with each request.
        var instanceCopy = new InstanceDto
        {
            InstanceGuid = Instance.InstanceGuid,
            InstanceIp = Instance.InstanceIp,
            InstanceName = _config.InstanceNameOverride ?? _appConfig.WebfrontCustomBranding,
            HeartBeat = DateTimeOffset.UtcNow,
            ApiKey = Instance.ApiKey,
            About = _appConfig.CommunityInformation.Description,
            Socials = _appConfig.CommunityInformation.SocialAccounts.ToDictionary(social => social.Title, social => social.Url),
        };

        _pluginEnabled = await EndpointManager.UpdateInstance(instanceCopy);

        // Unsubscribe from events if response is bad
        if (!_pluginEnabled)
        {
            IGameServerEventSubscriptions.MonitoringStarted -= OnMonitoringStarted;
            IManagementEventSubscriptions.Load -= OnLoad;
            IManagementEventSubscriptions.ClientStateAuthorized -= OnClientStateAuthorized;
            IManagementEventSubscriptions.ClientStateDisposed -= OnClientStateDisposed;
            IManagementEventSubscriptions.ClientPenaltyAdministered -= OnClientPenaltyAdministered;
            IManagementEventSubscriptions.ClientPenaltyRevoked -= OnClientPenaltyRevoked;

            Console.WriteLine($"[{ConfigurationModel.Name}] Failed to load. Is the API running?");
            return;
        }

        InstanceActive = await EndpointManager.IsInstanceActive(Instance);
        Instance.Active = InstanceActive;

        if (InstanceActive)
        {
            Console.WriteLine($"[{ConfigurationModel.Name}] Activated.");
            Console.WriteLine($"[{ConfigurationModel.Name}] Penalties and users will be reported to the API.");
        }
        else
        {
            Console.WriteLine($"[{ConfigurationModel.Name}] Not activated. Read-only access.");
            Console.WriteLine($"[{ConfigurationModel.Name}] To activate your access. Please visit https://discord.gg/Arruj6DWvp");
        }

        manager.QueueEvent(new NotifyAfterDelayRequestEvent
        {
            Source = "BanHub",
            DelayMs = 240_000,
        });
    }

    private async Task OnNotifyAfterDelayCompleted(NotifyAfterDelayCompleteEvent notifyEvent, CancellationToken token)
    {
        Console.WriteLine($"[{ConfigurationModel.Name}] Notify Event Fired");
        if (notifyEvent.Source is not "BanHub") return;
        await _heartBeatManager.InstanceHeartBeat();
        await _heartBeatManager.ClientHeartBeat();
        Manager.QueueEvent(new NotifyAfterDelayRequestEvent
        {
            Source = "BanHub",
            DelayMs = 240_000
        });
    }
}
