using BanHub.Configuration;
using BanHub.Managers;
using BanHub.Models;
using BanHub.Services;
using BanHub.Utilities;
using BanHubData.Commands.Instance;
using BanHubData.Commands.Instance.Server;
using BanHubData.Enums;
using Microsoft.Extensions.DependencyInjection;
using SharedLibraryCore;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Events.Management;
using SharedLibraryCore.Events.Server;
using SharedLibraryCore.Interfaces;
using SharedLibraryCore.Interfaces.Events;

namespace BanHub;

// TODO: LOGGING!!!!

public class Plugin : IPluginV2
{
    public string Name => "Ban Hub";
    public string Version => "2023-07-16";
    public string Author => "Amos";

    public static bool InstanceActive { get; private set; }
    private readonly InstanceSlim _instanceSlim;
    private readonly EndpointManager _endpointManager;
    private readonly HeartBeatManager _heartBeatManager;
    private readonly BanHubConfiguration _config;
    private readonly ApplicationConfiguration _appConfig;
    private readonly WhitelistManager _whitelistManager;

    public Plugin(InstanceSlim instanceSlim, HeartBeatManager heartBeatManager, EndpointManager endpointManager, BanHubConfiguration config,
        ApplicationConfiguration appConfig, WhitelistManager whitelistManager)
    {
        _config = config;
        _appConfig = appConfig;
        _whitelistManager = whitelistManager;
        _heartBeatManager = heartBeatManager;
        _endpointManager = endpointManager;
        _instanceSlim = instanceSlim;

        if (!config.EnableBanHub) return; // disable if not enabled in config

        IGameServerEventSubscriptions.MonitoringStarted += OnMonitoringStarted;
        IManagementEventSubscriptions.Load += OnLoad;
        IManagementEventSubscriptions.ClientStateAuthorized += OnClientStateAuthorized;
        IManagementEventSubscriptions.ClientStateDisposed += OnClientStateDisposed;
        IManagementEventSubscriptions.ClientPenaltyAdministered += OnClientPenaltyAdministered;
        IManagementEventSubscriptions.ClientPenaltyRevoked += OnClientPenaltyRevoked;
    }

    public static void RegisterDependencies(IServiceCollection serviceCollection)
    {
        serviceCollection.AddConfiguration("BanHubSettings", new BanHubConfiguration());

        serviceCollection.AddSingleton<HeartbeatService>();
        serviceCollection.AddSingleton<PlayerService>();
        serviceCollection.AddSingleton<PenaltyService>();
        serviceCollection.AddSingleton<InstanceService>();
        serviceCollection.AddSingleton<ServerService>();
        serviceCollection.AddSingleton(new InstanceSlim());
        serviceCollection.AddSingleton<HeartBeatManager>();
        serviceCollection.AddSingleton<EndpointManager>();
        serviceCollection.AddSingleton<WhitelistManager>();
    }

    private async Task OnMonitoringStarted(MonitorStartEvent startEvent, CancellationToken token)
    {
        var serverDto = new CreateOrUpdateServerCommand
        {
            ServerId = startEvent.Server.Id,
            ServerName = startEvent.Server.ServerName.StripColors(),
            ServerIp = startEvent.Server.ListenAddress,
            ServerPort = startEvent.Server.ListenPort,
            ServerGame = Enum.Parse<Game>(startEvent.Server.GameCode.ToString()),
            InstanceGuid = _instanceSlim.InstanceGuid
        };
        await _endpointManager.OnStart(serverDto);
    }

    private async Task OnClientPenaltyRevoked(ClientPenaltyRevokeEvent penaltyEvent, CancellationToken arg2) =>
        await AddPlayerPenaltyAsync(penaltyEvent);

    private async Task OnClientPenaltyAdministered(ClientPenaltyEvent penaltyEvent, CancellationToken arg2) =>
        await AddPlayerPenaltyAsync(penaltyEvent);

    private async Task AddPlayerPenaltyAsync(ClientPenaltyEvent penaltyEvent)
    {
        if (penaltyEvent.Penalty.Offender.GetAdditionalProperty<bool>("BanHubGlobalBan")) return;
        if (await _whitelistManager.IsWhitelisted(penaltyEvent.Client.ToPartialClient())) return;

        await _endpointManager.AddPlayerPenaltyAsync(penaltyEvent.Penalty.Type.ToString(),
            penaltyEvent.Penalty.Punisher.ToPartialClient(),
            penaltyEvent.Penalty.Offender.ToPartialClient(),
            penaltyEvent.Penalty.Offense,
            expiration: penaltyEvent.Penalty.Expires);
    }

    private Task OnClientStateDisposed(ClientStateEvent clientEvent, CancellationToken arg2)
    {
        // I think the lifetime of clientEvent will not remove it during this iteration. It'll be removed on the next person who leaves.
        // Maybe fix?
        _endpointManager.RemoveFromProfiles();
        return Task.CompletedTask;
    }

    private async Task OnClientStateAuthorized(ClientStateAuthorizeEvent clientEvent, CancellationToken arg2)
    {
        if (await _whitelistManager.IsWhitelisted(clientEvent.Client.ToPartialClient())) return;
        await _endpointManager.OnJoin(clientEvent.Client);
    }

    private async Task OnLoad(IManager manager, CancellationToken arg2)
    {
        Console.WriteLine(_config.DebugMode
            ? $"[{BanHubConfiguration.Name}] Loading... v{Version} !! DEBUG MODE !!"
            : $"[{BanHubConfiguration.Name}] Loading... v{Version}");

        // Update the instance and check its state (Singleton)
        _instanceSlim.InstanceGuid = Guid.Parse(_appConfig.Id);
        _instanceSlim.InstanceIp = manager.ExternalIPAddress;
        _instanceSlim.ApiKey = _config.ApiKey;

        var portRaw = _appConfig.WebfrontBindUrl.Split(":").LastOrDefault();
        _ = int.TryParse(portRaw, out var port);

        // We need a copy of this since we don't really want the other values being sent with each request.
        var instanceCopy = new CreateOrUpdateInstanceCommand
        {
            InstanceGuid = _instanceSlim.InstanceGuid,
            InstanceIp = _instanceSlim.InstanceIp,
            InstanceWebsite = _config.InstanceWebsite.GetDomainName(),
            InstanceBindPort = port,
            InstanceApiKey = _instanceSlim.ApiKey,
            InstanceName = _config.InstanceNameOverride ?? _appConfig.WebfrontCustomBranding ?? _instanceSlim.InstanceGuid.ToString(),
            About = _appConfig.CommunityInformation.Description,
            Socials = _appConfig.CommunityInformation.SocialAccounts.ToDictionary(social => social.Title, social => social.Url),
        };

        var enabled = await _endpointManager.CreateOrUpdateInstanceAsync(instanceCopy);

        // Issue creating instance. Unload.
        if (!enabled)
        {
            UnloadPlugin("Failed to load. Is the API running?");
            return;
        }

        InstanceActive = await _endpointManager.IsInstanceActive(_instanceSlim.InstanceGuid);
        _instanceSlim.Active = InstanceActive;

        if (InstanceActive)
        {
            Console.WriteLine($"[{BanHubConfiguration.Name}] Your instance is active. Penalties and users will be reported to the API.");
        }
        else
        {
            Console.WriteLine($"[{BanHubConfiguration.Name}] Not activated. Read-only access.");
            Console.WriteLine($"[{BanHubConfiguration.Name}] To activate your access. Please visit https://discord.gg/Arruj6DWvp");
        }

        SharedLibraryCore.Utilities.ExecuteAfterDelay(TimeSpan.FromMinutes(4), OnNotifyAfterDelayCompleted, CancellationToken.None);
    }

    private void UnloadPlugin(string message)
    {
        IGameServerEventSubscriptions.MonitoringStarted -= OnMonitoringStarted;
        IManagementEventSubscriptions.Load -= OnLoad;
        IManagementEventSubscriptions.ClientStateAuthorized -= OnClientStateAuthorized;
        IManagementEventSubscriptions.ClientStateDisposed -= OnClientStateDisposed;
        IManagementEventSubscriptions.ClientPenaltyAdministered -= OnClientPenaltyAdministered;
        IManagementEventSubscriptions.ClientPenaltyRevoked -= OnClientPenaltyRevoked;

        Console.WriteLine($"[{BanHubConfiguration.Name}] {message}");
    }

    private async Task OnNotifyAfterDelayCompleted(CancellationToken token)
    {
        await _heartBeatManager.InstanceHeartBeat();
        await _heartBeatManager.ClientHeartBeat();
        SharedLibraryCore.Utilities.ExecuteAfterDelay(TimeSpan.FromMinutes(4), OnNotifyAfterDelayCompleted, CancellationToken.None);
    }
}
