using BanHub.Configuration;
using BanHubData.Commands.Heartbeat;
using BanHubData.Enums;
using BanHubData.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using SharedLibraryCore.Interfaces;

namespace BanHub.SignalR;

public class PluginHub : IAsyncDisposable
{
    private readonly ILogger<PluginHub> _logger;
    public event Action<BroadcastGlobalBan, IManager>? OnGlobalBan;
    public event Action<ActivateCommunity>? OnActivateCommunity;
    private HubConnection? _hubConnection;
    private IManager? _manager;

    public PluginHub(ILogger<PluginHub> logger)
    {
        _logger = logger;
    }

    public async Task InitializeAsync(IManager manager)
    {
        _manager = manager;
        CreateHubConnection();
        await StartConnection();
        SubscribeToHubEvents();
    }

    private void CreateHubConnection()
    {
        _hubConnection = new HubConnectionBuilder()
#if DEBUG
            .WithUrl("http://localhost:8123/SignalR/PluginHub")
#else
            .WithUrl("https://banhub.gg/SignalR/PluginHub")
#endif
            .WithAutomaticReconnect()
            .Build();
    }

    private async Task StartConnection()
    {
        if (_hubConnection is not null) await _hubConnection.StartAsync();
    }

    private void SubscribeToHubEvents()
    {
        if (_manager is null) return;
        _hubConnection?.On<BroadcastGlobalBan>(SignalRMethods.PluginMethods.OnGlobalBan, ban => OnGlobalBan?.Invoke(ban, _manager));
        _hubConnection?.On<ActivateCommunity>(SignalRMethods.PluginMethods.ActivateCommunity,
            community => OnActivateCommunity?.Invoke(community));
    }

    public async Task Heartbeat(CommunityHeartbeatCommand communityHeartbeat)
    {
        if (_hubConnection is null) return;

        var result = await _hubConnection.InvokeAsync<SignalREnums.ReturnState>(SignalRMethods.PluginMethods.CommunityHeartbeat,
            communityHeartbeat);
        HandleSignalRReturn(result);
    }

    public async Task Heartbeat(PlayersHeartbeatCommand playersHeartbeat)
    {
        if (_hubConnection is null) return;

        var result = await _hubConnection.InvokeAsync<SignalREnums.ReturnState>(SignalRMethods.PluginMethods.PlayersHeartbeat,
            playersHeartbeat);
        HandleSignalRReturn(result);
    }

    public async Task Heartbeat(PlayerJoined player)
    {
        if (_hubConnection is null) return;

        var result = await _hubConnection.InvokeAsync<SignalREnums.ReturnState>(SignalRMethods.PluginMethods.PlayerJoined, player);
        HandleSignalRReturn(result);
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is {State: HubConnectionState.Connected})
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
        }
    }

    private void HandleSignalRReturn(SignalREnums.ReturnState state)
    {
        switch (state)
        {
            case SignalREnums.ReturnState.Ok:
                break;
            case SignalREnums.ReturnState.VersionToOld:
                Console.WriteLine($"[{BanHubConfiguration.Name}] BanHub Plugin is out of date. Please update to the latest version.");
                _logger.LogWarning("Plugin is out of date. Please update to the latest version");
                break;
            case SignalREnums.ReturnState.NotFound:
                Console.WriteLine($"[{BanHubConfiguration.Name}] Community not found. Please check your API key.");
                _logger.LogWarning("Community not found. Please check your API key");
                break;
            case SignalREnums.ReturnState.NotActivated:
                _logger.LogInformation("Community not activated");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }
}
