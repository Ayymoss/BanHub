using Microsoft.AspNetCore.SignalR.Client;

namespace BanHub.WebCore.Client.Services;

public class ActiveUserService
{
    public int ActiveUsers { get; private set; }
    public event Action<int>? ActiveUserCountChanged;
    private HubConnection _hubConnection = null!;

    public async Task Initialize()
    {
        _hubConnection = new HubConnectionBuilder()
#if DEBUG
            .WithUrl("http://localhost:8123/ActiveUsersHub")
#else
            .WithUrl("https://banhub.gg/ActiveUsersHub")
#endif
            .AddJsonProtocol()
            .Build();

        await _hubConnection.StartAsync();

        _hubConnection.On<int>("ActiveUserCountChanged", count =>
        {
            ActiveUsers = count;
            ActiveUserCountChanged?.Invoke(count);
        });
    }

    public async Task DecrementCountAsync()
    {
        ActiveUsers--;
        await _hubConnection.InvokeAsync("DecrementActiveUserCount");
    }
}
