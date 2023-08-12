using BanHub.Configuration;
using BanHub.Managers;
using BanHub.Models;
using BanHub.Services;
using BanHubData.Commands.Community;
using BanHubData.Commands.Community.Server;
using BanHubData.Enums;
using Microsoft.Extensions.DependencyInjection;
using SharedLibraryCore;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Events.Game;
using SharedLibraryCore.Events.Management;
using SharedLibraryCore.Events.Server;
using SharedLibraryCore.Interfaces;
using SharedLibraryCore.Interfaces.Events;

namespace BanHub;

// TODO: LOGGING!!!!

public class Plugin : IPluginV2
{
    public string Name => "Ban Hub";
    public string Version => "2023.08.11.01";
    public string Author => "Amos";

    private readonly CommunitySlim _communitySlim;
    private readonly EndpointManager _endpointManager;
    private readonly HeartbeatManager _heartbeatManager;
    private readonly BanHubConfiguration _config;
    private readonly ApplicationConfiguration _appConfig;
    private readonly WhitelistManager _whitelistManager;

    public Plugin(CommunitySlim communitySlim, HeartbeatManager heartbeatManager, EndpointManager endpointManager,
        BanHubConfiguration config, ApplicationConfiguration appConfig, WhitelistManager whitelistManager)
    {
        _config = config;
        _appConfig = appConfig;
        _whitelistManager = whitelistManager;
        _heartbeatManager = heartbeatManager;
        _endpointManager = endpointManager;
        _communitySlim = communitySlim;

        if (!config.EnableBanHub) return;

        IGameServerEventSubscriptions.MonitoringStarted += OnMonitoringStarted;
        IGameEventSubscriptions.ClientMessaged += OnChatMessaged;
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
        serviceCollection.AddSingleton<ChatService>();
        serviceCollection.AddSingleton<PlayerService>();
        serviceCollection.AddSingleton<PenaltyService>();
        serviceCollection.AddSingleton<CommunityService>();
        serviceCollection.AddSingleton<ServerService>();
        serviceCollection.AddSingleton<NoteService>();

        serviceCollection.AddSingleton(new CommunitySlim());
        serviceCollection.AddSingleton<HeartbeatManager>();
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
            CommunityGuid = _communitySlim.CommunityGuid
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
            expiration: penaltyEvent.Penalty.Expires?.Second is 0 ? null : penaltyEvent.Penalty.Expires);
    }

    private async Task OnClientStateAuthorized(ClientStateAuthorizeEvent clientEvent, CancellationToken arg2)
    {
        if (await _whitelistManager.IsWhitelisted(clientEvent.Client.ToPartialClient())) return;
        await _endpointManager.OnJoin(clientEvent.Client);
    }

    private Task OnClientStateDisposed(ClientStateEvent clientEvent, CancellationToken arg2)
    {
        // I think the lifetime of clientEvent will not remove it during this iteration. It'll be removed on the next person who leaves.
        // Maybe fix?
        _endpointManager.RemoveFromProfiles(clientEvent.Client);
        return Task.CompletedTask;
    }

    private async Task OnChatMessaged(ClientMessageEvent messageEvent, CancellationToken token)
    {
        if (!_communitySlim.Active) return;
        await _endpointManager.HandleChatMessageAsync(messageEvent, token);
    }

    private async Task OnLoad(IManager manager, CancellationToken arg2)
    {
        Console.WriteLine(_config.DebugMode
            ? $"[{BanHubConfiguration.Name}] Loading... v{Version} !! DEBUG MODE !!"
            : $"[{BanHubConfiguration.Name}] Loading... v{Version}");

        // Update the instance and check its state (Singleton)
        _communitySlim.CommunityGuid = Guid.Parse(_appConfig.Id);
        _communitySlim.CommunityIp = manager.ExternalIPAddress;
        _communitySlim.ApiKey = _config.ApiKey;

        var portRaw = _appConfig.WebfrontBindUrl
            .Replace("/", "")
            .Split(":")
            .LastOrDefault();
        _ = int.TryParse(portRaw, out var port);

        // We need a copy of this since we don't really want the other values being sent with each request.
        var instanceCopy = new CreateOrUpdateCommunityCommand
        {
            PluginVersion = new Version(Version),
            CommunityGuid = _communitySlim.CommunityGuid,
            CommunityIp = _communitySlim.CommunityIp,
            CommunityWebsite = _config.CommunityWebsite,
            CommunityPort = port,
            CommunityApiKey = _communitySlim.ApiKey,
            CommunityName = _config.CommunityNameOverride ?? _appConfig.WebfrontCustomBranding ?? _communitySlim.CommunityGuid.ToString(),
            About = _appConfig.CommunityInformation.Description,
            Socials = _appConfig.CommunityInformation.SocialAccounts.ToDictionary(social => social.Title, social => social.Url),
        };

        var enabled = await _endpointManager.CreateOrUpdateCommunityAsync(instanceCopy);

        // Issue creating instance. Unload.
        if (!enabled)
        {
            UnloadPlugin("Failed to load. Is the API running?");
            return;
        }

        _communitySlim.Active = await _endpointManager.IsCommunityActive(_communitySlim.CommunityGuid);

        if (_communitySlim.Active)
        {
            Console.WriteLine($"[{BanHubConfiguration.Name}] Your instance is active. Penalties and users will be reported to the API.");
        }
        else
        {
            Console.WriteLine($"[{BanHubConfiguration.Name}] Not activated. Read-only access.");
            Console.WriteLine($"[{BanHubConfiguration.Name}] To activate your access. Please visit https://discord.gg/Arruj6DWvp");
        }

        _endpointManager.RegisterInteraction(manager);
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
        await _heartbeatManager.CommunityHeartbeat(Version);
        if (_communitySlim.Active) await _heartbeatManager.ClientHeartbeat(Version);
        SharedLibraryCore.Utilities.ExecuteAfterDelay(TimeSpan.FromMinutes(4), OnNotifyAfterDelayCompleted, CancellationToken.None);
    }
}
