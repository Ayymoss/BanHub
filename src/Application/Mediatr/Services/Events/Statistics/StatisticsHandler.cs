using BanHub.Application.Interfaces;
using BanHub.Domain.Enums;
using BanHub.Domain.Interfaces.Repositories;
using BanHub.Domain.Interfaces.Services;
using BanHub.Domain.ValueObjects.Plugin.SignalR;
using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Services.Events.Statistics;

public class StatisticsHandler(IStatisticsCache statisticsCache, ISignalRNotification signalRNotification,
        IPublisher publisher, IPlayerRepository playerRepository, ICommunityRepository communityRepository,
        IServerRepository serverRepository, IPenaltyRepository penaltyRepository)
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
                    await playerRepository.GetPlayerCountAsync(cancellationToken: cancellationToken));
                statisticsCache.SetStatisticsCount(ControllerEnums.StatisticType.CommunityCount,
                    await communityRepository.GetCommunityCountAsync(cancellationToken: cancellationToken));
                statisticsCache.SetStatisticsCount(ControllerEnums.StatisticType.ServerCount,
                    await serverRepository.GetServerCountAsync(cancellationToken: cancellationToken));
                statisticsCache.SetStatisticsCount(ControllerEnums.StatisticType.PenaltyCount,
                    await penaltyRepository.GetPenaltyCountAsync(cancellationToken: cancellationToken));
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

        await signalRNotification.NotifyUserAsync(HubType.Statistics, SignalRMethods.StatisticMethods.OnPlayerCountUpdate,
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

        await signalRNotification.NotifyUserAsync(HubType.Statistics, SignalRMethods.StatisticMethods.OnRecentBansUpdate,
            statisticsCache.RecentBans.Count,
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
