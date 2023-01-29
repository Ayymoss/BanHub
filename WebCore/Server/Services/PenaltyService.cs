using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Models;
using BanHub.WebCore.Shared.DTOs;
using BanHub.WebCore.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Services;

public class PenaltyService : IPenaltyService
{
    private readonly IDiscordWebhookService _discordWebhook;
    private readonly IStatisticService _statisticService;
    private readonly ILogger _logger;
    private readonly DataContext _context;
    private readonly IEntityService _entityService;

    public PenaltyService(DataContext context, IEntityService entityService, IDiscordWebhookService discordWebhook,
        IStatisticService statisticService, ILogger<PenaltyService> logger)
    {
        _discordWebhook = discordWebhook;
        _statisticService = statisticService;
        _logger = logger;
        _context = context;
        _entityService = entityService;
    }

    public async Task<(ControllerEnums.ProfileReturnState, Guid?)> AddPenalty(PenaltyDto request)
    {
        await _entityService.CreateOrUpdate(request.Target!);
        await _entityService.CreateOrUpdate(request.Admin!);

        var user = await _context.Entities
            .AsTracking()
            .Include(x => x.CurrentAlias.Alias)
            .FirstOrDefaultAsync(entity => entity.Identity == request.Target!.Identity);

        var admin = await _context.Entities
            .FirstOrDefaultAsync(profile => profile.Identity == request.Admin!.Identity);

        if (user is null || admin is null) return (ControllerEnums.ProfileReturnState.NotFound, null);

        // Check if the user has an existing ban and the incoming is an unban from the server that banned them
        var activeGlobalBan = await _context.Penalties
            .AsTracking()
            .Where(inf => inf.TargetId == user.Id && inf.Instance.InstanceGuid == request.Instance!.InstanceGuid)
            .FirstOrDefaultAsync(inf =>
                inf.PenaltyType == PenaltyType.Ban
                && inf.PenaltyScope == PenaltyScope.Global
                && inf.PenaltyStatus == PenaltyStatus.Active);

        // Already global banned - don't want to create another
        if (activeGlobalBan is not null && request.PenaltyType == PenaltyType.Ban)
        {
            return (ControllerEnums.ProfileReturnState.NotModified, null);
        }

        var penalty = await _context.Penalties
            .AsTracking()
            .Where(inf =>
                inf.TargetId == user.Id && inf.Instance.InstanceGuid == request.Instance!.InstanceGuid
                                        && (inf.PenaltyType == PenaltyType.Ban || inf.PenaltyType == PenaltyType.TempBan)
                                        && inf.PenaltyStatus == PenaltyStatus.Active)
            .ToListAsync();

        if (!penalty.Any())
        {
            switch (request.PenaltyType)
            {
                // If the incoming request is an unban, unban them.
                case PenaltyType.Unban:
                    foreach (var inf in penalty)
                    {
                        inf.PenaltyStatus = PenaltyStatus.Revoked;
                        _context.Update(penalty);
                    }

                    break;
                // If they're already banned. We don't need to keep creating kick infractions.
                case PenaltyType.Kick:
                    return (ControllerEnums.ProfileReturnState.NotModified, null);
            }
        }
        else
        {
            switch (request.PenaltyType)
            {
                // Trying to unban when no record of a ban exists.
                case PenaltyType.Unban:
                    return (ControllerEnums.ProfileReturnState.NotModified, null);
            }
        }

        var instance = await _context.Instances.FirstOrDefaultAsync(x => x.ApiKey == request.Instance!.ApiKey);
        if (instance is null) return (ControllerEnums.ProfileReturnState.NotFound, null);

        var penaltyModel = new EFPenalty
        {
            PenaltyType = request.PenaltyType!.Value,
            PenaltyStatus = request.PenaltyType is PenaltyType.Ban or PenaltyType.TempBan
                ? PenaltyStatus.Active
                : PenaltyStatus.Informational,
            PenaltyScope = request.PenaltyScope!.Value,
            PenaltyGuid = Guid.NewGuid(),
            Submitted = DateTimeOffset.UtcNow,
            AdminId = admin.Id,
            Reason = request.Reason!,
            Duration = request.Duration,
            Evidence = request.Evidence,
            InstanceId = instance.Id,
            TargetId = user.Id
        };

        await _statisticService.UpdateStatistic(ControllerEnums.StatisticType.PenaltyCount);
        _context.Add(penaltyModel);
        await _context.SaveChangesAsync();

        try
        {
            await _discordWebhook.CreatePenaltyHook(penaltyModel.PenaltyScope, penaltyModel.PenaltyType,
                penaltyModel.PenaltyGuid,
                user.Identity, user.CurrentAlias.Alias.UserName, penaltyModel.Reason);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return (ControllerEnums.ProfileReturnState.Created, penaltyModel.PenaltyGuid);
    }

    public async Task<bool> RevokeGlobalBan(PenaltyDto request)
    {
        await _entityService.CreateOrUpdate(request.Target!);
        await _entityService.CreateOrUpdate(request.Admin!);

        var user = await _context.Entities
            .AsTracking()
            .Include(x => x.CurrentAlias.Alias)
            .FirstOrDefaultAsync(entity => entity.Identity == request.Target!.Identity);

        var admin = await _context.Entities
            .FirstOrDefaultAsync(profile => profile.Identity == request.Admin!.Identity);

        if (user is null || admin is null) return false;

        var globalBans = await _context.Penalties
            .AsTracking()
            .Where(inf =>
                inf.TargetId == user.Id && inf.Instance.InstanceGuid == request.Instance!.InstanceGuid
                                        && inf.PenaltyType == PenaltyType.Ban
                                        && inf.PenaltyStatus == PenaltyStatus.Active)
            .ToListAsync();

        if (!globalBans.Any()) return false;

        // Remove any actives
        foreach (var inf in globalBans)
        {
            inf.PenaltyStatus = PenaltyStatus.Revoked;
            _context.Penalties.Update(inf);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(ControllerEnums.ProfileReturnState, PenaltyDto?)> GetPenalty(string penaltyGuid)
    {
        var parseGuid = Guid.TryParse(penaltyGuid, out var guid);
        if (!parseGuid) return (ControllerEnums.ProfileReturnState.BadRequest, null);

        var penalty = await _context.Penalties.Where(inf => inf.PenaltyGuid == guid).Select(inf => new PenaltyDto
        {
            PenaltyGuid = inf.PenaltyGuid,
            PenaltyType = inf.PenaltyType,
            PenaltyStatus = inf.PenaltyStatus,
            PenaltyScope = inf.PenaltyScope,
            Submitted = inf.Submitted,
            Admin = new EntityDto
            {
                Identity = inf.Admin.Identity,
                Alias = new AliasDto
                {
                    UserName = inf.Admin.CurrentAlias.Alias.UserName,
                }
            },
            Reason = inf.Reason,
            Evidence = inf.Evidence,
            Duration = inf.Duration,
            Instance = new InstanceDto
            {
                InstanceName = inf.Instance.InstanceName,
                InstanceGuid = inf.Instance.InstanceGuid
            },
            Target = new EntityDto
            {
                Identity = inf.Target.Identity,
                Alias = new AliasDto
                {
                    UserName = inf.Target.CurrentAlias.Alias.UserName,
                }
            }
        }).FirstOrDefaultAsync();

        return penalty is null
            ? (ControllerEnums.ProfileReturnState.NotFound, null)
            : (ControllerEnums.ProfileReturnState.Ok, penalty);
    }

    public async Task<(ControllerEnums.ProfileReturnState, List<PenaltyDto>?)> GetPenalties()
    {
        var penalties = await _context.Penalties.Select(inf => new PenaltyDto
        {
            PenaltyGuid = inf.PenaltyGuid,
            PenaltyType = inf.PenaltyType,
            PenaltyStatus = inf.PenaltyStatus,
            PenaltyScope = inf.PenaltyScope,
            Submitted = inf.Submitted,
            Admin = new EntityDto
            {
                Identity = inf.Admin.Identity,
                Alias = new AliasDto
                {
                    UserName = inf.Admin.CurrentAlias.Alias.UserName,
                }
            },
            Reason = inf.Reason,
            Evidence = inf.Evidence,
            Duration = inf.Duration,
            Instance = new InstanceDto
            {
                InstanceName = inf.Instance.InstanceName,
                InstanceGuid = inf.Instance.InstanceGuid
            },
            Target = new EntityDto
            {
                Identity = inf.Target.Identity,
                Alias = new AliasDto
                {
                    UserName = inf.Target.CurrentAlias.Alias.UserName,
                }
            }
        }).ToListAsync();

        penalties = penalties.OrderByDescending(x => x.Submitted).ToList();

        return penalties.Count is 0
            ? (ControllerEnums.ProfileReturnState.Ok, new List<PenaltyDto>())
            : (ControllerEnums.ProfileReturnState.Ok, penalties);
    }

    public async Task<int> GetPenaltyDayCount()
    {
        return await _context.Penalties
            .Where(x => x.Submitted + TimeSpan.FromDays(1) > DateTimeOffset.UtcNow
                        && x.PenaltyType == PenaltyType.Ban
                        && x.PenaltyScope == PenaltyScope.Global
                        && x.PenaltyStatus == PenaltyStatus.Active)
            .CountAsync();
    }

    public async Task<bool> SubmitEvidence(PenaltyDto request)
    {
        var penalty = await _context.Penalties
            .AsTracking()
            .FirstOrDefaultAsync(x => x.PenaltyGuid == request.PenaltyGuid);

        if (penalty is null) return false;

        penalty.Evidence = request.Evidence;
        _context.Penalties.Update(penalty);
        await _context.SaveChangesAsync();
        return true;
    }
}
