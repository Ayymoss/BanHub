using BanHubData.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using SharedLibraryCore.Interfaces;

namespace BanHub.SignalR;

public class PluginHub : IAsyncDisposable
{
    public event Action<BroadcastGlobalBan, IManager>? OnGlobalBan;
    private HubConnection? _hubConnection;
    private IManager? _manager;

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
        _hubConnection?.On<BroadcastGlobalBan>(HubMethods.OnGlobalBan, ban => OnGlobalBan?.Invoke(ban, _manager));
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is {State: HubConnectionState.Connected})
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
        }
    }
}
