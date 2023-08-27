using BanHubData.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace BanHub.WebCore.Client.SignalR;

public class TomatoCounterHub : IAsyncDisposable
{
    public event Action<int>? TomatoCountChanged;
    private HubConnection? _hubConnection;

    public async Task InitializeAsync(string identity)
    {
        CreateHubConnection();
        await StartConnection();
        SubscribeToHubEvents();
        await FetchInitialCounts(identity);
    }

    private void CreateHubConnection()
    {
        _hubConnection = new HubConnectionBuilder()
#if DEBUG
            .WithUrl("http://localhost:8123/SignalR/TomatoCounterHub")
#else
            .WithUrl("https://banhub.gg/SignalR/TomatoCounterHub")
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
        _hubConnection?.On<int>(SignalRMethods.TomatoMethods.OnTomatoCountUpdate, count => TomatoCountChanged?.Invoke(count));
    }

    public async Task IncrementCount(string identity)
    {
        if (_hubConnection is not null) await _hubConnection.SendAsync(SignalRMethods.TomatoMethods.IncrementTomatoCount, identity);
    }

    private async Task FetchInitialCounts(string identity)
    {
        if (_hubConnection is null) return;
        var count = await _hubConnection.InvokeAsync<int>(SignalRMethods.TomatoMethods.GetTomatoCount, identity);
        TomatoCountChanged?.Invoke(count);
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
