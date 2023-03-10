using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Models;
using BanHub.WebCore.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Services;

public class HeartBeatService : IHeartBeatService
{
    private readonly DataContext _context;
    private readonly IStatisticService _statisticService;

    public HeartBeatService(DataContext context, IStatisticService statisticService)
    {
        _context = context;
        _statisticService = statisticService;
    }

    public async Task<(ControllerEnums.ReturnState, bool)> InstanceHeartbeatAsync(InstanceDto request)
    {
        var instance = await _context.Instances
            .AsTracking()
            .FirstOrDefaultAsync(x => x.InstanceGuid == request.InstanceGuid && x.ApiKey == request.ApiKey);
        if (instance is null) return (ControllerEnums.ReturnState.NotFound, false);

        instance.HeartBeat = DateTimeOffset.UtcNow;
        _context.Instances.Update(instance);
        await _context.SaveChangesAsync();
        return (ControllerEnums.ReturnState.Updated, instance.Active);
    }

    public async Task EntitiesHeartbeatAsync(List<EntityDto> request)
    {
        var profiles = await _context.Entities
            .AsTracking()
            .Where(p => request
                .Select(r => r.Identity)
                .Contains(p.Identity))
            .ToListAsync();

        foreach (var profile in profiles)
        {
            profile.HeartBeat = DateTimeOffset.UtcNow;
            profile.PlayTime = profile.PlayTime.Add(new TimeSpan(0, 0, 4, 0));
            _context.Entities.Update(profile);
        }

        var count = request.Count;
        if (count is not 0 && request[0].Instance is not null)
        {
            await _statisticService.UpdateOnlineStatisticAsync(new StatisticUsersOnline
            {
                InstanceGuid = request[0].Instance!.InstanceGuid,
                Online = count,
                HeartBeat = DateTimeOffset.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }
}
