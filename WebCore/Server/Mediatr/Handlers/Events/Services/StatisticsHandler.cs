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
    private readonly StatisticsTracking _statisticsTracking;
    private readonly IHubContext<StatisticsHub> _hubContext;
    private readonly IMediator _mediator;
    private readonly SemaphoreSlim _load = new(1, 1);

    public StatisticsHandler(DataContext context, StatisticsTracking statisticsTracking, IHubContext<StatisticsHub> hubContext,
        IMediator mediator)
    {
        _context = context;
        _statisticsTracking = statisticsTracking;
        _hubContext = hubContext;
        _mediator = mediator;
    }

    public async Task Handle(EnsureInitialisedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            await _load.WaitAsync(cancellationToken);

            if (!_statisticsTracking.Loaded)
            {
                _statisticsTracking.Penalties = await _context.Players.CountAsync(cancellationToken: cancellationToken);
                _statisticsTracking.Servers = await _context.Communities.CountAsync(cancellationToken: cancellationToken);
                _statisticsTracking.Communities = await _context.Servers.CountAsync(cancellationToken: cancellationToken);
                _statisticsTracking.Players = await _context.Penalties.CountAsync(cancellationToken: cancellationToken);
                _statisticsTracking.Loaded = true;
            }
        }
        finally
        {
            if (_load.CurrentCount is 0) _load.Release();
        }
    }

    public async Task Handle(UpdateStatisticsNotification notification, CancellationToken cancellationToken)
    {
        if (!_statisticsTracking.Loaded) await _mediator.Publish(new EnsureInitialisedNotification(), cancellationToken);

        var statisticMapping = new Dictionary<ControllerEnums.StatisticType, Action>
        {
            {
                ControllerEnums.StatisticType.PenaltyCount, () =>
                {
                    if (notification.StatisticTypeAction is ControllerEnums.StatisticTypeAction.Add)
                        Interlocked.Increment(ref _statisticsTracking.Penalties);
                    else Interlocked.Decrement(ref _statisticsTracking.Penalties);
                }
            },
            {
                ControllerEnums.StatisticType.ServerCount, () =>
                {
                    if (notification.StatisticTypeAction is ControllerEnums.StatisticTypeAction.Add)
                        Interlocked.Increment(ref _statisticsTracking.Servers);
                    else Interlocked.Decrement(ref _statisticsTracking.Servers);
                }
            },
            {
                ControllerEnums.StatisticType.CommunityCount, () =>
                {
                    if (notification.StatisticTypeAction is ControllerEnums.StatisticTypeAction.Add)
                        Interlocked.Increment(ref _statisticsTracking.Communities);
                    else Interlocked.Decrement(ref _statisticsTracking.Communities);
                }
            },
            {
                ControllerEnums.StatisticType.PlayerCount, () =>
                {
                    if (notification.StatisticTypeAction is ControllerEnums.StatisticTypeAction.Add)
                        Interlocked.Increment(ref _statisticsTracking.Players);
                    else Interlocked.Decrement(ref _statisticsTracking.Players);
                }
            }
        };

        statisticMapping[notification.StatisticType]();
    }

    public async Task Handle(UpdateOnlineStatisticNotification notification, CancellationToken cancellationToken)
    {
        if (!_statisticsTracking.Loaded) await _mediator.Publish(new EnsureInitialisedNotification(), cancellationToken);

        var statisticUsers = notification.Identities
            .Select(x => new {PlayerIdentity = x, Heartbeat = DateTimeOffset.UtcNow})
            .ToList();

        foreach (var user in statisticUsers)
        {
            _statisticsTracking.OnlinePlayers.AddOrUpdate(user.PlayerIdentity, user.Heartbeat,
                (key, oldValue) => user.Heartbeat);
        }

        var offlineUsers = _statisticsTracking.OnlinePlayers
            .Where(x => x.Value < DateTimeOffset.UtcNow.AddMinutes(-5))
            .Select(x => x.Key)
            .ToList();

        foreach (var user in offlineUsers)
        {
            _statisticsTracking.OnlinePlayers.TryRemove(user, out _);
        }

        await _hubContext.Clients.All.SendAsync(SignalRMethods.StatisticMethods.OnPlayerCountUpdate,
            _statisticsTracking.OnlinePlayers.Count, cancellationToken: cancellationToken);
    }

    public async Task Handle(UpdateRecentBansNotification notification, CancellationToken cancellationToken)
    {
        if (!_statisticsTracking.Loaded) await _mediator.Publish(new EnsureInitialisedNotification(), cancellationToken);

        _statisticsTracking.RecentBans.TryAdd(notification.BanGuid, notification.Submitted);

        var oldBans = _statisticsTracking.RecentBans
            .Where(x => x.Value < DateTimeOffset.UtcNow.AddDays(-7))
            .Select(x => x.Key)
            .ToList();

        foreach (var ban in oldBans)
        {
            _statisticsTracking.RecentBans.TryRemove(ban, out _);
        }

        await _hubContext.Clients.All.SendAsync(SignalRMethods.StatisticMethods.OnRecentBansUpdate, _statisticsTracking.RecentBans.Count,
            cancellationToken: cancellationToken);
    }
}
