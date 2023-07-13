using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Models;
using BanHub.WebCore.Shared.ViewModels;
using BanHubData.Domains;
using BanHubData.Enums;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Services;

public class StatisticService : IStatisticService
{
    private readonly DataContext _context;
    private readonly StatisticsTracking _statisticsTracking;

    private readonly SemaphoreSlim _load = new(1, 1);

    public StatisticService(DataContext context, StatisticsTracking statisticsTracking)
    {
        _context = context;
        _statisticsTracking = statisticsTracking;
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
                _statisticsTracking.Instances = result.InstanceCount;
                _statisticsTracking.Entities = result.EntityCount;
                _statisticsTracking.Loaded = true;
            }
        }
        finally
        {
            if (_load.CurrentCount == 0) _load.Release();
        }
    }

    public async Task UpdateStatisticAsync(ControllerEnums.StatisticType statistic, ControllerEnums.StatisticTypeAction action)
    {
        if (!_statisticsTracking.Loaded) await EnsureInitialisedAsync();

        var actionMapping = new Dictionary<ControllerEnums.StatisticTypeAction, Action<int>>
        {
            {ControllerEnums.StatisticTypeAction.Add, x => Interlocked.Add(ref x, 1)},
            {ControllerEnums.StatisticTypeAction.Remove, x => Interlocked.Add(ref x, -1)}
        };

        var statisticMapping = new Dictionary<ControllerEnums.StatisticType, Action>
        {
            {ControllerEnums.StatisticType.PenaltyCount, () => actionMapping[action](_statisticsTracking.Penalties)},
            {ControllerEnums.StatisticType.ServerCount, () => actionMapping[action](_statisticsTracking.Servers)},
            {ControllerEnums.StatisticType.InstanceCount, () => actionMapping[action](_statisticsTracking.Instances)},
            {ControllerEnums.StatisticType.EntityCount, () => actionMapping[action](_statisticsTracking.Entities)}
        };

        statisticMapping[statistic]();
    }

    public async Task<Statistic> GetStatisticsAsync()
    {
        if (!_statisticsTracking.Loaded) await EnsureInitialisedAsync();

        return new Statistic()
        {
            PenaltyCount = _statisticsTracking.Penalties,
            ServerCount = _statisticsTracking.Servers,
            InstanceCount = _statisticsTracking.Instances,
            EntityCount = _statisticsTracking.Entities,
            OnlineCount = _statisticsTracking.UsersOnlineCount,
            BanCount = _statisticsTracking.BansDayCount
        };
    }

    public async Task UpdateOnlineStatisticAsync(StatisticUsersOnline statisticUsers)
    {
        if (!_statisticsTracking.Loaded) await EnsureInitialisedAsync();

        var instance = _statisticsTracking.UsersOnline
            .FirstOrDefault(x => x.InstanceGuid == statisticUsers.InstanceGuid);

        if (instance is null)
        {
            _statisticsTracking.UsersOnline.Add(statisticUsers);
        }
        else
        {
            instance.Online = statisticUsers.Online;
            instance.HeartBeat = statisticUsers.HeartBeat;
        }

        _statisticsTracking.UsersOnline = _statisticsTracking.UsersOnline
            .Where(x => x.HeartBeat > DateTimeOffset.UtcNow.AddMinutes(-5))
            .ToList();

        _statisticsTracking.UsersOnlineCount = _statisticsTracking.UsersOnline.Sum(x => x.Online);
    }

    public async Task UpdateDayStatisticAsync(StatisticBan statisticBan)
    {
        if (!_statisticsTracking.Loaded) await EnsureInitialisedAsync();

        _statisticsTracking.BansDay.Add(statisticBan);

        _statisticsTracking.BansDay = _statisticsTracking.BansDay
            .Where(x => x.Submitted > DateTimeOffset.UtcNow.AddDays(-1))
            .ToList();

        _statisticsTracking.BansDayCount = _statisticsTracking.BansDay.Count;
    }

    private async Task<Statistic> ReadStatisticsAsync()
    {
        var entity = await _context.Players.CountAsync();
        var instance = await _context.Instances.CountAsync();
        var server = await _context.Servers.CountAsync();
        var penalty = await _context.Penalties.CountAsync();

        var statistics = new Statistic
        {
            EntityCount = entity,
            InstanceCount = instance,
            ServerCount = server,
            PenaltyCount = penalty
        };

        return statistics;
    }
}
