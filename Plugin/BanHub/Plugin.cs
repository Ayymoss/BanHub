using System.Text;
using BanHub.Configuration;
using BanHub.Managers;
using BanHub.Models;
using BanHub.Services;
using BanHub.SignalR;
using BanHubData.Enums;
using BanHubData.Mediatr.Commands.Requests.Community;
using BanHubData.Mediatr.Commands.Requests.Community.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using SharedLibraryCore;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Events.Game;
using SharedLibraryCore.Events.Management;
using SharedLibraryCore.Events.Server;
using SharedLibraryCore.Interfaces;
using SharedLibraryCore.Interfaces.Events;

namespace BanHub;

public class Plugin : IPluginV2
{
    private const string LoggerName = "BanHub";
    public string Name => "Ban Hub";
    public string Version => "2023.08.27.01";
    public string Author => "Amos";

    private readonly CommunitySlim _communitySlim;
    private readonly EndpointManager _endpointManager;
    private readonly HeartbeatManager _heartbeatManager;
    private readonly BanHubConfiguration _config;
    private readonly ApplicationConfiguration _appConfig;
    private readonly WhitelistManager _whitelistManager;
    private readonly PluginHub _pluginHub;
    private readonly ILogger<Plugin> _logger;

    public Plugin(CommunitySlim communitySlim, HeartbeatManager heartbeatManager, EndpointManager endpointManager,
        BanHubConfiguration config, ApplicationConfiguration appConfig, WhitelistManager whitelistManager, PluginHub pluginHub,
        ILogger<Plugin> logger)
    {
        _config = config;
        _appConfig = appConfig;
        _whitelistManager = whitelistManager;
        _pluginHub = pluginHub;
        _logger = logger;
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
        serviceCollection.AddConfiguration("BanHubSettingsTest", new BanHubConfiguration());

        serviceCollection.AddSingleton(new CommunitySlim());
        serviceCollection.AddSingleton<PluginHub>();

        serviceCollection.AddSingleton<ChatService>();
        serviceCollection.AddSingleton<PlayerService>();
        serviceCollection.AddSingleton<PenaltyService>();
        serviceCollection.AddSingleton<CommunityService>();
        serviceCollection.AddSingleton<ServerService>();
        serviceCollection.AddSingleton<NoteService>();

        serviceCollection.AddSingleton<HeartbeatManager>();
        serviceCollection.AddSingleton<EndpointManager>();
        serviceCollection.AddSingleton<WhitelistManager>();
    }

    private async Task OnMonitoringStarted(MonitorStartEvent startEvent, CancellationToken token)
    {
        using (LogContext.PushProperty("Server", LoggerName))
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
    }

    private async Task OnClientPenaltyRevoked(ClientPenaltyRevokeEvent penaltyEvent, CancellationToken arg2)
    {
        using (LogContext.PushProperty("Server", LoggerName))
        {
            await AddPlayerPenaltyAsync(penaltyEvent);
        }
    }

    private async Task OnClientPenaltyAdministered(ClientPenaltyEvent penaltyEvent, CancellationToken arg2)
    {
        using (LogContext.PushProperty("Server", LoggerName))
        {
            await AddPlayerPenaltyAsync(penaltyEvent);
        }
    }

    private async Task AddPlayerPenaltyAsync(ClientPenaltyEvent penaltyEvent)
    {
        using (LogContext.PushProperty("Server", LoggerName))
        {
            if (penaltyEvent.Penalty.Offender.GetAdditionalProperty<bool>("BanHubGlobalBan")) return;
            if (await _whitelistManager.IsWhitelisted(penaltyEvent.Client.ToPartialClient())) return;

            var expiration = penaltyEvent.Penalty.Expires.HasValue &&
                             penaltyEvent.Penalty.Expires.Value - DateTime.UtcNow < TimeSpan.FromSeconds(1)
                ? null
                : penaltyEvent.Penalty.Expires;

            await _endpointManager.AddPlayerPenaltyAsync(penaltyEvent.Penalty.Type.ToString(),
                penaltyEvent.Penalty.Punisher.ToPartialClient(),
                penaltyEvent.Penalty.Offender.ToPartialClient(),
                penaltyEvent.Penalty.Offense,
                expiration: expiration);
        }
    }

    private async Task OnClientStateAuthorized(ClientStateAuthorizeEvent clientEvent, CancellationToken arg2)
    {
        using (LogContext.PushProperty("Server", LoggerName))
        {
            if (await _whitelistManager.IsWhitelisted(clientEvent.Client.ToPartialClient())) return;
            await _endpointManager.OnJoin(clientEvent.Client);
            await _heartbeatManager.PlayerJoinedAsync(EndpointManager.EntityToPlayerIdentity(clientEvent.Client), Version);
        }
    }

    private Task OnClientStateDisposed(ClientStateEvent clientEvent, CancellationToken arg2)
    {
        using (LogContext.PushProperty("Server", LoggerName))
        {
            _endpointManager.RemoveFromProfiles(clientEvent.Client);
            return Task.CompletedTask;
        }
    }

    private async Task OnChatMessaged(ClientMessageEvent messageEvent, CancellationToken token)
    {
        using (LogContext.PushProperty("Server", LoggerName))
        {
            if (!_communitySlim.Active) return;
            await _endpointManager.HandleChatMessageAsync(messageEvent, token);
        }
    }

    private async Task OnLoad(IManager manager, CancellationToken arg2)
    {
        using (LogContext.PushProperty("Server", LoggerName))
        {
            var loadingMessage = new StringBuilder();
            loadingMessage.Append($"[{BanHubConfiguration.Name}] Loading... v{Version}");

            Console.WriteLine(loadingMessage);

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
                CommunityName = _config.CommunityNameOverride ??
                                _appConfig.WebfrontCustomBranding ??
                                _communitySlim.CommunityGuid.ToString(),
                About = _appConfig.CommunityInformation.Description,
                Socials = _appConfig.CommunityInformation.SocialAccounts.ToDictionary(social => social.Title, social => social.Url),
            };

            var enabled = await _endpointManager.CreateOrUpdateCommunityAsync(instanceCopy);

            // Issue creating instance. Unload.
            if (!enabled)
            {
                UnloadPlugin();
                return;
            }

            _communitySlim.Active = await _endpointManager.IsCommunityActive(_communitySlim.CommunityGuid);

            Console.WriteLine(
                _communitySlim.Active
                    ? $"[{BanHubConfiguration.Name}] Your instance is active. Penalties and users will be reported to the API."
                    : $"[{BanHubConfiguration.Name}] Not activated. Activate here: https://discord.gg/Arruj6DWvp");

            await _pluginHub.InitializeAsync(manager);
            _pluginHub.OnGlobalBan += _endpointManager.OnGlobalBan;
            _pluginHub.OnActivateCommunity += _endpointManager.OnActivateCommunity;

            _endpointManager.RegisterInteraction(manager);
            await HeartbeatScheduler(CancellationToken.None);
        }
    }

    private void UnloadPlugin()
    {
        IGameServerEventSubscriptions.MonitoringStarted -= OnMonitoringStarted;
        IManagementEventSubscriptions.Load -= OnLoad;
        IManagementEventSubscriptions.ClientStateAuthorized -= OnClientStateAuthorized;
        IManagementEventSubscriptions.ClientStateDisposed -= OnClientStateDisposed;
        IManagementEventSubscriptions.ClientPenaltyAdministered -= OnClientPenaltyAdministered;
        IManagementEventSubscriptions.ClientPenaltyRevoked -= OnClientPenaltyRevoked;

        Console.WriteLine($"[{BanHubConfiguration.Name}] Failed to load. Check with BanHub Support for help.");
        _logger.LogError("Failed to load. Unloading...");
    }

    private async Task HeartbeatScheduler(CancellationToken token)
    {
        await _heartbeatManager.CommunityHeartbeatAsync(Version);
        if (_communitySlim.Active) await _heartbeatManager.ClientHeartbeatAsync(Version);
        SharedLibraryCore.Utilities.ExecuteAfterDelay(TimeSpan.FromMinutes(4), HeartbeatScheduler, CancellationToken.None);
    }
}
