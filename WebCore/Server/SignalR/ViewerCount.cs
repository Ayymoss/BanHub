using Microsoft.AspNetCore.SignalR;

namespace BanHub.WebCore.Server.SignalR;

public class ViewerCount : Hub
{
    private static int _activeUserCount;

    public override async Task OnConnectedAsync()
    {
        _activeUserCount++;
        await Clients.All.SendAsync("ActiveUserCountChanged", _activeUserCount);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _activeUserCount--;
        await Clients.All.SendAsync("ActiveUserCountChanged", _activeUserCount);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task IncrementActiveUserCount()
    {
        _activeUserCount++;
        await Clients.All.SendAsync("ActiveUserCountChanged", _activeUserCount);
    }

    public async Task DecrementActiveUserCount()
    {
        _activeUserCount--;
        await Clients.All.SendAsync("ActiveUserCountChanged", _activeUserCount);
    }
}
