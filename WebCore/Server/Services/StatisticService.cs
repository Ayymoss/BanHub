using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Models;
using BanHub.WebCore.Server.SignalR;
using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Enums;
using BanHubData.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Services;

public class StatisticService : IStatisticService
{
    private readonly DataContext _context;
    private readonly StatisticsTracking _statisticsTracking;
    private readonly IHubContext<StatisticsHub> _hubContext;
    private readonly SemaphoreSlim _load = new(1, 1);

    public StatisticService(DataContext context, StatisticsTracking statisticsTracking, IHubContext<StatisticsHub> hubContext)
    {
        _context = context;
        _statisticsTracking = statisticsTracking;
        _hubContext = hubContext;
    }

    private async Task EnsureInitialisedAsync()
    {
        try
        {
            await _load.WaitAsync();

            if (!_statisticsTracking.Loaded)
            {
                var result = await ReadStatisticsAsync();

                _statisticsTracking.Penalties = result.PenaltyCount;
                _statisticsTracking.Servers = result.ServerCount;
                _statisticsTracking.Communities = result.CommunityCount;
                _statisticsTracking.Players = result.PlayerCount;
                _statisticsTracking.Loaded = true;
            }
        }
        finally
        {
            if (_load.CurrentCount is 0) _load.Release();
        }
    }

    public async Task UpdateStatisticAsync(ControllerEnums.StatisticType statistic, ControllerEnums.StatisticTypeAction action)
    {
        if (!_statisticsTracking.Loaded) await EnsureInitialisedAsync();

        var statisticMapping = new Dictionary<ControllerEnums.StatisticType, Action>
        {
            {
                ControllerEnums.StatisticType.PenaltyCount, () =>
                {
                    if (action is ControllerEnums.StatisticTypeAction.Add) Interlocked.Increment(ref _statisticsTracking.Penalties);
                    else Interlocked.Decrement(ref _statisticsTracking.Penalties);
                }
            },
            {
                ControllerEnums.StatisticType.ServerCount, () =>
                {
                    if (action is ControllerEnums.StatisticTypeAction.Add) Interlocked.Increment(ref _statisticsTracking.Servers);
                    else Interlocked.Decrement(ref _statisticsTracking.Servers);
                }
            },
            {
                ControllerEnums.StatisticType.CommunityCount, () =>
                {
                    if (action is ControllerEnums.StatisticTypeAction.Add) Interlocked.Increment(ref _statisticsTracking.Communities);
                    else Interlocked.Decrement(ref _statisticsTracking.Communities);
                }
            },
            {
                ControllerEnums.StatisticType.PlayerCount, () =>
                {
                    if (action is ControllerEnums.StatisticTypeAction.Add) Interlocked.Increment(ref _statisticsTracking.Players);
                    else Interlocked.Decrement(ref _statisticsTracking.Players);
                }
            }
        };

        statisticMapping[statistic]();
    }

    public async Task<Statistic> GetStatisticsAsync()
    {
        if (!_statisticsTracking.Loaded) await EnsureInitialisedAsync();

        return new Statistic
        {
            PenaltyCount = _statisticsTracking.Penalties,
            ServerCount = _statisticsTracking.Servers,
            CommunityCount = _statisticsTracking.Communities,
            PlayerCount = _statisticsTracking.Players,
        };
    }

    public async Task UpdateOnlineStatisticAsync(IEnumerable<string> playerIdentities)
    {
        if (!_statisticsTracking.Loaded) await EnsureInitialisedAsync();

        var statisticUsers = playerIdentities
            .Select(x => new {PlayerIdentity = x, Heartbeat = DateTimeOffset.UtcNow})
            .ToList();

        foreach (var user in statisticUsers)
        {
            _statisticsTracking.OnlinePlayers.AddOrUpdate(user.PlayerIdentity, user.Heartbeat,
                (key, oldValue) => user.Heartbeat);
        }

        var offlineUsers = _statisticsTracking.OnlinePlayers
            .Where(x => x.Value < DateTimeOffset.UtcNow.AddMinutes(-4))
            .Select(x => x.Key)
            .ToList();

        foreach (var user in offlineUsers)
        {
            _statisticsTracking.OnlinePlayers.TryRemove(user, out _);
        }

        await _hubContext.Clients.All.SendAsync(HubMethods.OnPlayerCountUpdate, _statisticsTracking.OnlinePlayers.Count);
    }

    public async Task UpdateRecentBansStatisticAsync(StatisticBan statisticBan)
    {
        if (!_statisticsTracking.Loaded) await EnsureInitialisedAsync();

        _statisticsTracking.RecentBans.TryAdd(statisticBan.BanGuid, statisticBan.Submitted);

        var oldBans = _statisticsTracking.RecentBans
            .Where(x => x.Value < DateTimeOffset.UtcNow.AddDays(-7))
            .Select(x => x.Key)
            .ToList();

        foreach (var ban in oldBans)
        {
            _statisticsTracking.RecentBans.TryRemove(ban, out _);
        }

        await _hubContext.Clients.All.SendAsync(HubMethods.OnRecentBansUpdate, _statisticsTracking.RecentBans.Count);
    }

    public int GetOnlinePlayerCount() => _statisticsTracking.OnlinePlayers.Count;
    public int GetRecentBansCount() => _statisticsTracking.RecentBans.Count;

    private async Task<Statistic> ReadStatisticsAsync()
    {
        var player = await _context.Players.CountAsync();
        var instance = await _context.Communities.CountAsync();
        var server = await _context.Servers.CountAsync();
        var penalty = await _context.Penalties.CountAsync();

        var statistics = new Statistic
        {
            PlayerCount = player,
            CommunityCount = instance,
            ServerCount = server,
            PenaltyCount = penalty,
        };

        return statistics;
    }
}
