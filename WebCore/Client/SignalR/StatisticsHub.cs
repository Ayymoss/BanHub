using BanHubData.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace BanHub.WebCore.Client.SignalR;

public class StatisticsHub : IAsyncDisposable
{
    public event Action<int>? OnlineCountChanged;
    public event Action<int>? RecentBansCountChanged;
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
            .WithUrl("http://localhost:8123/SignalR/StatisticsHub")
#else
            .WithUrl("https://banhub.gg/SignalR/StatisticsHub")
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
        _hubConnection?.On<int>(HubMethods.OnPlayerCountUpdate, count => OnlineCountChanged?.Invoke(count));
        _hubConnection?.On<int>(HubMethods.OnRecentBansUpdate, count => RecentBansCountChanged?.Invoke(count));
    }

    private async Task FetchInitialCounts()
    {
        if (_hubConnection is null) return;
        var onlineCount = await _hubConnection.InvokeAsync<int>(HubMethods.GetCurrentOnlinePlayers);
        var bansCount = await _hubConnection.InvokeAsync<int>(HubMethods.GetCurrentRecentBans);
        OnlineCountChanged?.Invoke(onlineCount);
        RecentBansCountChanged?.Invoke(bansCount);
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
