using Microsoft.AspNetCore.SignalR;

namespace BanHub.WebCore.Server.SignalR;

public class ViewerCount : Hub
{
    private static int _activeUserCount;
    private static readonly object ActiveUserCountLock = new();

    public override async Task OnConnectedAsync()
    {
        lock (ActiveUserCountLock)
        {
            UpdateUserCount(UserCountAction.Increment);
        }

        await Clients.All.SendAsync("ActiveUserCountChanged", _activeUserCount);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        lock (ActiveUserCountLock)
        {
            UpdateUserCount(UserCountAction.Decrement);
        }

        await Clients.All.SendAsync("ActiveUserCountChanged", _activeUserCount);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task IncrementActiveUserCount()
    {
        lock (ActiveUserCountLock)
        {
            UpdateUserCount(UserCountAction.Increment);
        }

        await Clients.All.SendAsync("ActiveUserCountChanged", _activeUserCount);
    }

    public async Task DecrementActiveUserCount()
    {
        lock (ActiveUserCountLock)
        {
            UpdateUserCount(UserCountAction.Decrement);
        }

        await Clients.All.SendAsync("ActiveUserCountChanged", _activeUserCount);
    }

    private static void UpdateUserCount(UserCountAction action)
    {
        switch (action)
        {
            case UserCountAction.Increment:
                _activeUserCount++;
                break;
            case UserCountAction.Decrement:
                _activeUserCount--;
                break;
        }

        _activeUserCount = _activeUserCount < 0 ? 0 : _activeUserCount;
    }

    private enum UserCountAction
    {
        Increment,
        Decrement
    }
}
