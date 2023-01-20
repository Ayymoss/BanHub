using GlobalInfraction.WebCore.Server.Context;
using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Server.Interfaces;
using GlobalInfraction.WebCore.Server.Models;
using GlobalInfraction.WebCore.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GlobalInfraction.WebCore.Server.Services;

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

                _statisticsTracking.Infractions = result.InfractionCount!.Value;
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
            case ControllerEnums.StatisticType.InfractionCount:
                Interlocked.Increment(ref _statisticsTracking.Infractions);
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
            InfractionCount = _statisticsTracking.Infractions,
            ServerCount = _statisticsTracking.Servers,
            InstanceCount = _statisticsTracking.Instances,
            EntityCount = _statisticsTracking.Entities
        };
    }

    private async Task<StatisticDto> ReadStatistics()
    {
        var entity = await _context.Statistics.FirstAsync(x => x.Id == (int)ControllerEnums.StatisticType.EntityCount);
        var infraction = await _context.Statistics.FirstAsync(x => x.Id == (int)ControllerEnums.StatisticType.InfractionCount);
        var instance = await _context.Statistics.FirstAsync(x => x.Id == (int)ControllerEnums.StatisticType.InstanceCount);
        var server = await _context.Statistics.FirstAsync(x => x.Id == (int)ControllerEnums.StatisticType.ServerCount);
        var statistics = new StatisticDto
        {
            EntityCount = entity.Count,
            InstanceCount = instance.Count,
            ServerCount = server.Count,
            InfractionCount = infraction.Count
        };
        return statistics;
    }

    private async Task WriteStatistics()
    {
        var entity = await _context.Statistics.FirstAsync(x => x.Id == (int)ControllerEnums.StatisticType.EntityCount);
        var infraction = await _context.Statistics.FirstAsync(x => x.Id == (int)ControllerEnums.StatisticType.InfractionCount);
        var instance = await _context.Statistics.FirstAsync(x => x.Id == (int)ControllerEnums.StatisticType.InstanceCount);
        var server = await _context.Statistics.FirstAsync(x => x.Id == (int)ControllerEnums.StatisticType.ServerCount);

        entity.Count = _statisticsTracking.Entities;
        infraction.Count = _statisticsTracking.Infractions;
        instance.Count = _statisticsTracking.Instances;
        server.Count = _statisticsTracking.Servers;

        _context.Statistics.Update(entity);
        _context.Statistics.Update(infraction);
        _context.Statistics.Update(instance);
        _context.Statistics.Update(server);

        await _context.SaveChangesAsync();
    }
}
