using BanHub.Domain.ValueObjects.Plugin.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace BanHub.WebCore.Client.SignalR;

public class ActiveUserHub : IAsyncDisposable
{
    public event Action<int>? ActiveUserCountChanged;
    private HubConnection? _hubConnection;

    public async Task InitializeAsync()
    {
        CreateHubConnection();
        await StartConnection();
        SubscribeToHubEvents();
        await FetchInitialCounts();
    }

    private void CreateHubConnection()
    {
        _hubConnection = new HubConnectionBuilder()
#if DEBUG
            .WithUrl("http://localhost:8123/SignalR/ActiveUsersHub")
#else
            .WithUrl("https://banhub.gg/SignalR/ActiveUsersHub")
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
        _hubConnection?.On<int>(SignalRMethods.ActiveMethods.OnActiveUsersUpdate, count => ActiveUserCountChanged?.Invoke(count));
    }

    private async Task FetchInitialCounts()
    {
        if (_hubConnection is null) return;
        var onlineCount = await _hubConnection.InvokeAsync<int>(SignalRMethods.ActiveMethods.GetActiveUsersCount);
        ActiveUserCountChanged?.Invoke(onlineCount);
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
