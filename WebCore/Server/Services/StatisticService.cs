using GlobalInfraction.WebCore.Server.Context;
using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Server.Interfaces;
using GlobalInfraction.WebCore.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GlobalInfraction.WebCore.Server.Services;

public class StatisticService : IStatisticService
{
    private readonly DataContext _context;

    public StatisticService(DataContext context)
    {
        _context = context;
    }

    public async Task<StatisticDto> GetStatistics()
    {
        var entity = await _context.Statistics.FirstAsync(x => x.Id == (int)ControllerEnums.StatisticType.EntityCount);
        var alias = await _context.Statistics.FirstAsync(x => x.Id == (int)ControllerEnums.StatisticType.AliasCount);
        var infraction = await _context.Statistics.FirstAsync(x => x.Id == (int)ControllerEnums.StatisticType.InfractionCount);
        var instance = await _context.Statistics.FirstAsync(x => x.Id == (int)ControllerEnums.StatisticType.InstanceCount);
        var server = await _context.Statistics.FirstAsync(x => x.Id == (int)ControllerEnums.StatisticType.ServerCount);
        var statistics = new StatisticDto
        {
            EntityCount = entity.Count,
            InstanceCount = instance.Count,
            AliasCount = alias.Count,
            ServerCount = server.Count,
            InfractionCount = infraction.Count
        };
        return statistics;
    }
}
