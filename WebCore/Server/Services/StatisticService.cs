using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Services;

public class StatisticService : IStatisticService
{
    private readonly DataContext _context;
    private readonly StatisticsTracking _statisticsTracking;

    private readonly SemaphoreSlim _load = new(1, 1);
    private readonly SemaphoreSlim _save = new(1, 1);

    public StatisticService(DataContext context, StatisticsTracking statisticsTracking)
    {
        _context = context;
        _statisticsTracking = statisticsTracking;
    }


    private async Task EnsureInitialised()
    {
        try
        {
            await _load.WaitAsync();

            if (!_statisticsTracking.Loaded)
            {
                var result = await ReadStatistics();

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

    public async Task UpdateStatistic(ControllerEnums.StatisticType statistic)
    {
        if (!_statisticsTracking.Loaded) await EnsureInitialised();

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

        if (_statisticsTracking.LastSave + TimeSpan.FromSeconds(10) < DateTimeOffset.UtcNow)
        {
            try
            {
                await _save.WaitAsync();
                
                if (_statisticsTracking.LastSave + TimeSpan.FromSeconds(10) < DateTimeOffset.UtcNow)
                {
                    await WriteStatistics();
                    _statisticsTracking.LastSave = DateTimeOffset.UtcNow;
                }
            }
            finally
            {
                if (_save.CurrentCount == 0) _save.Release();
            }
        }
    }

    public async Task<StatisticDto> GetStatistics()
    {
        if (!_statisticsTracking.Loaded) await EnsureInitialised();

        return new StatisticDto
        {
            PenaltyCount = _statisticsTracking.Penalties,
            ServerCount = _statisticsTracking.Servers,
            InstanceCount = _statisticsTracking.Instances,
            EntityCount = _statisticsTracking.Entities
        };
    }

    private async Task<StatisticDto> ReadStatistics()
    {
        var entity = await _context.Statistics.FirstAsync(x => x.Id == (int)ControllerEnums.StatisticType.EntityCount);
        var penalty = await _context.Statistics.FirstAsync(x => x.Id == (int)ControllerEnums.StatisticType.PenaltyCount);
        var instance = await _context.Statistics.FirstAsync(x => x.Id == (int)ControllerEnums.StatisticType.InstanceCount);
        var server = await _context.Statistics.FirstAsync(x => x.Id == (int)ControllerEnums.StatisticType.ServerCount);
        var statistics = new StatisticDto
        {
            EntityCount = entity.Count,
            InstanceCount = instance.Count,
            ServerCount = server.Count,
            PenaltyCount = penalty.Count
        };
        return statistics;
    }

    private async Task WriteStatistics()
    {
        var entity = await _context.Statistics.FirstAsync(x => x.Id == (int)ControllerEnums.StatisticType.EntityCount);
        var penalty = await _context.Statistics.FirstAsync(x => x.Id == (int)ControllerEnums.StatisticType.PenaltyCount);
        var instance = await _context.Statistics.FirstAsync(x => x.Id == (int)ControllerEnums.StatisticType.InstanceCount);
        var server = await _context.Statistics.FirstAsync(x => x.Id == (int)ControllerEnums.StatisticType.ServerCount);

        entity.Count = _statisticsTracking.Entities;
        penalty.Count = _statisticsTracking.Penalties;
        instance.Count = _statisticsTracking.Instances;
        server.Count = _statisticsTracking.Servers;

        _context.Statistics.Update(entity);
        _context.Statistics.Update(penalty);
        _context.Statistics.Update(instance);
        _context.Statistics.Update(server);

        await _context.SaveChangesAsync();
    }
}
