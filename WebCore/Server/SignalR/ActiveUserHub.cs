﻿using BanHubData.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace BanHub.WebCore.Server.SignalR;

public class ActiveUserHub(ILogger logger) : Hub
{
    private static int _activeUserCount;
    private readonly SemaphoreSlim _countLock = new(1, 1);

    public int GetActiveUsersCount() => _activeUserCount;

    private async Task UpdateAndBroadcastCount(int change)
    {
        try
        {
            await _countLock.WaitAsync();
            _activeUserCount = Math.Max(0, _activeUserCount + change);
        }
        finally
        {
            if (_countLock.CurrentCount is 0) _countLock.Release();
        }

        await Clients.All.SendAsync(SignalRMethods.ActiveMethods.OnActiveUsersUpdate, _activeUserCount);
    }

    public override async Task OnConnectedAsync()
    {
        await UpdateAndBroadcastCount(1);
        logger.LogDebug("{ConnectionId} connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await UpdateAndBroadcastCount(-1);
        await base.OnDisconnectedAsync(exception);
    }
}
