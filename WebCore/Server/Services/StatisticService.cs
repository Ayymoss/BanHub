using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Models;
using BanHub.WebCore.Shared.DTOs;
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

                _statisticsTracking.Penalties = result.PenaltyCount!.Value;
                _statisticsTracking.Servers = result.ServerCount!.Value;
                _statisticsTracking.Instances = result.InstanceCount!.Value;
                _statisticsTracking.Entities = result.EntityCount!.Value;
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

        // TODO: Rewrite this. Ugh
        switch (action)
        {
            case ControllerEnums.StatisticTypeAction.Add:
                switch (statistic)
                {
                    case ControllerEnums.StatisticType.PenaltyCount:
                        Interlocked.Increment(ref _statisticsTracking.Penalties);
                        break;
                    case ControllerEnums.StatisticType.ServerCount:
                        Interlocked.Increment(ref _statisticsTracking.Servers);
                        break;
                    case ControllerEnums.StatisticType.InstanceCount:
                        Interlocked.Increment(ref _statisticsTracking.Instances);
                        break;
                    case ControllerEnums.StatisticType.EntityCount:
                        Interlocked.Increment(ref _statisticsTracking.Entities);
                        break;
                }

                break;
            case ControllerEnums.StatisticTypeAction.Remove:
                switch (statistic)
                {
                    case ControllerEnums.StatisticType.PenaltyCount:
                        Interlocked.Decrement(ref _statisticsTracking.Penalties);
                        break;
                    case ControllerEnums.StatisticType.ServerCount:
                        Interlocked.Decrement(ref _statisticsTracking.Servers);
                        break;
                    case ControllerEnums.StatisticType.InstanceCount:
                        Interlocked.Decrement(ref _statisticsTracking.Instances);
                        break;
                    case ControllerEnums.StatisticType.EntityCount:
                        Interlocked.Decrement(ref _statisticsTracking.Entities);
                        break;
                }

                break;
        }
    }

    public async Task<StatisticDto> GetStatisticsAsync()
    {
        if (!_statisticsTracking.Loaded) await EnsureInitialisedAsync();

        return new StatisticDto
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

    private async Task<StatisticDto> ReadStatisticsAsync()
    {
        var entity = await _context.Entities.CountAsync();
        var instance = await _context.Instances.CountAsync();
        var server = await _context.Servers.CountAsync();
        var penalty = await _context.Penalties.CountAsync();

        var statistics = new StatisticDto
        {
            EntityCount = entity,
            InstanceCount = instance,
            ServerCount = server,
            PenaltyCount = penalty
        };

        return statistics;
    }
}
