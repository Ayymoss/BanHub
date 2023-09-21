using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Mediatr.Commands.Events.Services.Statistics;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Server.SignalR;
using BanHubData.Enums;
using BanHubData.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Events.Services;

public class StatisticsHandler(DataContext context, IStatisticsCache statisticsCache, IHubContext<StatisticsHub> hubContext,
        IPublisher publisher)
    : INotificationHandler<EnsureInitialisedNotification>, INotificationHandler<UpdateStatisticsNotification>,
        INotificationHandler<UpdateOnlineStatisticNotification>, INotificationHandler<UpdateRecentBansNotification>,
        INotificationHandler<UpdatePlayerServerStatisticNotification>
{
    private readonly SemaphoreSlim _load = new(1, 1);

    public async Task Handle(EnsureInitialisedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            await _load.WaitAsync(cancellationToken);

            if (!statisticsCache.Loaded)
            {
                statisticsCache.SetStatisticsCount(ControllerEnums.StatisticType.PlayerCount,
                    await context.Players.CountAsync(cancellationToken: cancellationToken));
                statisticsCache.SetStatisticsCount(ControllerEnums.StatisticType.CommunityCount,
                    await context.Communities.CountAsync(cancellationToken: cancellationToken));
                statisticsCache.SetStatisticsCount(ControllerEnums.StatisticType.ServerCount,
                    await context.Servers.CountAsync(cancellationToken: cancellationToken));
                statisticsCache.SetStatisticsCount(ControllerEnums.StatisticType.PenaltyCount,
                    await context.Penalties.CountAsync(cancellationToken: cancellationToken));
                statisticsCache.Loaded = true;
            }
        }
        finally
        {
            if (_load.CurrentCount is 0) _load.Release();
        }
    }

    public async Task Handle(UpdateStatisticsNotification notification, CancellationToken cancellationToken)
    {
        if (!statisticsCache.Loaded) await publisher.Publish(new EnsureInitialisedNotification(), cancellationToken);
        statisticsCache.UpdateStatisticCount(notification.StatisticType, notification.StatisticTypeAction);
    }

    public async Task Handle(UpdateOnlineStatisticNotification notification, CancellationToken cancellationToken)
    {
        if (!statisticsCache.Loaded) await publisher.Publish(new EnsureInitialisedNotification(), cancellationToken);

        var statisticUsers = notification.Identities
            .Select(x => new {PlayerIdentity = x, Heartbeat = DateTimeOffset.UtcNow})
            .ToList();

        foreach (var user in statisticUsers)
        {
            statisticsCache.OnlinePlayers.AddOrUpdate(user.PlayerIdentity, user.Heartbeat,
                (key, oldValue) => user.Heartbeat);
        }

        var offlineUsers = statisticsCache.OnlinePlayers
            .Where(x => x.Value < DateTimeOffset.UtcNow.AddMinutes(-5))
            .Select(x => x.Key)
            .ToList();

        foreach (var user in offlineUsers)
        {
            statisticsCache.OnlinePlayers.TryRemove(user, out _);
        }

        await hubContext.Clients.All.SendAsync(SignalRMethods.StatisticMethods.OnPlayerCountUpdate,
            statisticsCache.OnlinePlayers.Count, cancellationToken: cancellationToken);
    }

    public async Task Handle(UpdateRecentBansNotification notification, CancellationToken cancellationToken)
    {
        if (!statisticsCache.Loaded) await publisher.Publish(new EnsureInitialisedNotification(), cancellationToken);

        statisticsCache.RecentBans.TryAdd(notification.BanGuid, notification.Submitted);

        var oldBans = statisticsCache.RecentBans
            .Where(x => x.Value < DateTimeOffset.UtcNow.AddDays(-7))
            .Select(x => x.Key)
            .ToList();

        foreach (var ban in oldBans)
        {
            statisticsCache.RecentBans.TryRemove(ban, out _);
        }

        await hubContext.Clients.All.SendAsync(SignalRMethods.StatisticMethods.OnRecentBansUpdate, statisticsCache.RecentBans.Count,
            cancellationToken: cancellationToken);
    }

    public Task Handle(UpdatePlayerServerStatisticNotification notification, CancellationToken cancellationToken)
    {
        statisticsCache.OnlineServers.AddOrUpdate(notification.ServerId, new ServerOnlineStatistic
        {
            OnlineCount = 1,
            LastUpdated = DateTimeOffset.UtcNow
        }, (key, oldValue) =>
        {
            oldValue.OnlineCount++;
            oldValue.LastUpdated = DateTimeOffset.UtcNow;
            return oldValue;
        });

        return Task.CompletedTask;
    }
}
