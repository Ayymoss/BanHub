using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Mediatr.Commands.Events.Statistics;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Server.SignalR;
using BanHubData.Enums;
using BanHubData.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Events.Services;

public class StatisticsHandler : INotificationHandler<EnsureInitialisedNotification>,
    INotificationHandler<UpdateStatisticsNotification>, INotificationHandler<UpdateOnlineStatisticNotification>,
    INotificationHandler<UpdateRecentBansNotification>
{
    private readonly DataContext _context;
    private readonly StatisticsCache _statisticsCache;
    private readonly IHubContext<StatisticsHub> _hubContext;
    private readonly IMediator _mediator;
    private readonly SemaphoreSlim _load = new(1, 1);

    public StatisticsHandler(DataContext context, StatisticsCache statisticsCache, IHubContext<StatisticsHub> hubContext,
        IMediator mediator)
    {
        _context = context;
        _statisticsCache = statisticsCache;
        _hubContext = hubContext;
        _mediator = mediator;
    }

    public async Task Handle(EnsureInitialisedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            await _load.WaitAsync(cancellationToken);

            if (!_statisticsCache.Loaded)
            {
                _statisticsCache.Penalties = await _context.Players.CountAsync(cancellationToken: cancellationToken);
                _statisticsCache.Servers = await _context.Communities.CountAsync(cancellationToken: cancellationToken);
                _statisticsCache.Communities = await _context.Servers.CountAsync(cancellationToken: cancellationToken);
                _statisticsCache.Players = await _context.Penalties.CountAsync(cancellationToken: cancellationToken);
                _statisticsCache.Loaded = true;
            }
        }
        finally
        {
            if (_load.CurrentCount is 0) _load.Release();
        }
    }

    public async Task Handle(UpdateStatisticsNotification notification, CancellationToken cancellationToken)
    {
        if (!_statisticsCache.Loaded) await _mediator.Publish(new EnsureInitialisedNotification(), cancellationToken);

        var statisticMapping = new Dictionary<ControllerEnums.StatisticType, Action>
        {
            {
                ControllerEnums.StatisticType.PenaltyCount, () =>
                {
                    if (notification.StatisticTypeAction is ControllerEnums.StatisticTypeAction.Add)
                        Interlocked.Increment(ref _statisticsCache.Penalties);
                    else Interlocked.Decrement(ref _statisticsCache.Penalties);
                }
            },
            {
                ControllerEnums.StatisticType.ServerCount, () =>
                {
                    if (notification.StatisticTypeAction is ControllerEnums.StatisticTypeAction.Add)
                        Interlocked.Increment(ref _statisticsCache.Servers);
                    else Interlocked.Decrement(ref _statisticsCache.Servers);
                }
            },
            {
                ControllerEnums.StatisticType.CommunityCount, () =>
                {
                    if (notification.StatisticTypeAction is ControllerEnums.StatisticTypeAction.Add)
                        Interlocked.Increment(ref _statisticsCache.Communities);
                    else Interlocked.Decrement(ref _statisticsCache.Communities);
                }
            },
            {
                ControllerEnums.StatisticType.PlayerCount, () =>
                {
                    if (notification.StatisticTypeAction is ControllerEnums.StatisticTypeAction.Add)
                        Interlocked.Increment(ref _statisticsCache.Players);
                    else Interlocked.Decrement(ref _statisticsCache.Players);
                }
            }
        };

        statisticMapping[notification.StatisticType]();
    }

    public async Task Handle(UpdateOnlineStatisticNotification notification, CancellationToken cancellationToken)
    {
        if (!_statisticsCache.Loaded) await _mediator.Publish(new EnsureInitialisedNotification(), cancellationToken);

        var statisticUsers = notification.Identities
            .Select(x => new {PlayerIdentity = x, Heartbeat = DateTimeOffset.UtcNow})
            .ToList();

        foreach (var user in statisticUsers)
        {
            _statisticsCache.OnlinePlayers.AddOrUpdate(user.PlayerIdentity, user.Heartbeat,
                (key, oldValue) => user.Heartbeat);
        }

        var offlineUsers = _statisticsCache.OnlinePlayers
            .Where(x => x.Value < DateTimeOffset.UtcNow.AddMinutes(-5))
            .Select(x => x.Key)
            .ToList();

        foreach (var user in offlineUsers)
        {
            _statisticsCache.OnlinePlayers.TryRemove(user, out _);
        }

        await _hubContext.Clients.All.SendAsync(SignalRMethods.StatisticMethods.OnPlayerCountUpdate,
            _statisticsCache.OnlinePlayers.Count, cancellationToken: cancellationToken);
    }

    public async Task Handle(UpdateRecentBansNotification notification, CancellationToken cancellationToken)
    {
        if (!_statisticsCache.Loaded) await _mediator.Publish(new EnsureInitialisedNotification(), cancellationToken);

        _statisticsCache.RecentBans.TryAdd(notification.BanGuid, notification.Submitted);

        var oldBans = _statisticsCache.RecentBans
            .Where(x => x.Value < DateTimeOffset.UtcNow.AddDays(-7))
            .Select(x => x.Key)
            .ToList();

        foreach (var ban in oldBans)
        {
            _statisticsCache.RecentBans.TryRemove(ban, out _);
        }

        await _hubContext.Clients.All.SendAsync(SignalRMethods.StatisticMethods.OnRecentBansUpdate, _statisticsCache.RecentBans.Count,
            cancellationToken: cancellationToken);
    }
}
