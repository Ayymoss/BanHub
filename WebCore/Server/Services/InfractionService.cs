using System.Text.Json;
using GlobalInfraction.WebCore.Server.Context;
using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Server.Interfaces;
using GlobalInfraction.WebCore.Server.Models;
using GlobalInfraction.WebCore.Shared.Enums;
using GlobalInfraction.WebCore.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GlobalInfraction.WebCore.Server.Services;

public class InfractionService : IInfractionService
{
    private readonly IDiscordWebhookService _discordWebhook;
    private readonly IStatisticService _statisticService;
    private readonly ILogger _logger;
    private readonly DataContext _context;
    private readonly IEntityService _entityService;

    public InfractionService(DataContext context, IEntityService entityService, IDiscordWebhookService discordWebhook,
        IStatisticService statisticService, ILogger<InfractionService> logger)
    {
        _discordWebhook = discordWebhook;
        _statisticService = statisticService;
        _logger = logger;
        _context = context;
        _entityService = entityService;
    }

    public async Task<(ControllerEnums.ProfileReturnState, Guid?)> AddInfraction(InfractionDto request)
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
        var activeGlobalBan = await _context.Infractions
            .AsTracking()
            .Where(inf => inf.TargetId == user.Id && inf.Instance.InstanceGuid == request.Instance!.InstanceGuid)
            .FirstOrDefaultAsync(inf =>
                inf.InfractionType == InfractionType.Ban
                && inf.InfractionScope == InfractionScope.Global
                && inf.InfractionStatus == InfractionStatus.Active);

        // Already global banned - don't want to create another
        if (activeGlobalBan is not null && request.InfractionType == InfractionType.Ban)
        {
            return (ControllerEnums.ProfileReturnState.NotModified, null);
        }

        var infractions = await _context.Infractions
            .AsTracking()
            .Where(inf =>
                inf.TargetId == user.Id && inf.Instance.InstanceGuid == request.Instance!.InstanceGuid
                                        && (inf.InfractionType == InfractionType.Ban || inf.InfractionType == InfractionType.TempBan)
                                        && inf.InfractionStatus == InfractionStatus.Active)
            .ToListAsync();

        if (!infractions.Any())
        {
            switch (request.InfractionType)
            {
                // If the incoming request is an unban, unban them.
                case InfractionType.Unban:
                    foreach (var inf in infractions)
                    {
                        inf.InfractionStatus = InfractionStatus.Revoked;
                        _context.Update(infractions);
                    }

                    break;
                // If they're already banned. We don't need to keep creating kick infractions.
                case InfractionType.Kick:
                    return (ControllerEnums.ProfileReturnState.NotModified, null);
            }
        }
        else
        {
            switch (request.InfractionType)
            {
                // Trying to unban when no record of a ban exists.
                case InfractionType.Unban:
                    return (ControllerEnums.ProfileReturnState.NotModified, null);
            }
        }

        var instance = await _context.Instances.FirstOrDefaultAsync(x => x.ApiKey == request.Instance!.ApiKey);
        if (instance is null) return (ControllerEnums.ProfileReturnState.NotFound, null);

        var infractionModel = new EFInfraction
        {
            InfractionType = request.InfractionType!.Value,
            InfractionStatus = request.InfractionType is InfractionType.Ban or InfractionType.TempBan
                ? InfractionStatus.Active
                : InfractionStatus.Informational,
            InfractionScope = request.InfractionScope!.Value,
            InfractionGuid = Guid.NewGuid(),
            Submitted = DateTimeOffset.UtcNow,
            AdminId = admin.Id,
            Reason = request.Reason!,
            Duration = request.Duration,
            Evidence = request.Evidence,
            InstanceId = instance.Id,
            TargetId = user.Id
        };

        switch (request.InfractionType)
        {
            case InfractionType.Ban:
                user.Strike++;
                _context.Entities.Update(user);
                break;
            case InfractionType.Unban:
                user.Strike--;
                _context.Entities.Update(user);
                break;
        }

        await _statisticService.UpdateStatistic(ControllerEnums.StatisticType.InfractionCount);
        _context.Add(infractionModel);
        await _context.SaveChangesAsync();

        try
        {
            await _discordWebhook.CreateInfractionHook(infractionModel.InfractionScope, infractionModel.InfractionType,
                infractionModel.InfractionGuid,
                user.Identity, user.CurrentAlias.Alias.UserName, infractionModel.Reason);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return (ControllerEnums.ProfileReturnState.Created, infractionModel.InfractionGuid);
    }

    public async Task<bool> RevokeGlobalBan(InfractionDto request)
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

        var globalBans = await _context.Infractions
            .AsTracking()
            .Where(inf =>
                inf.TargetId == user.Id && inf.Instance.InstanceGuid == request.Instance!.InstanceGuid
                                        && inf.InfractionType == InfractionType.Ban
                                        && inf.InfractionStatus == InfractionStatus.Active)
            .ToListAsync();
        
        if (!globalBans.Any()) return false;

        // Remove any actives
        foreach (var inf in globalBans)
        {
            inf.InfractionStatus = InfractionStatus.Revoked;
            _context.Infractions.Update(inf);
            user.Strike--;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(ControllerEnums.ProfileReturnState, InfractionDto?)> GetInfraction(string infractionGuid)
    {
        var parseGuid = Guid.TryParse(infractionGuid, out var guid);
        if (!parseGuid) return (ControllerEnums.ProfileReturnState.BadRequest, null);

        var infraction = await _context.Infractions.Where(inf => inf.InfractionGuid == guid).Select(inf => new InfractionDto
        {
            InfractionGuid = inf.InfractionGuid,
            InfractionType = inf.InfractionType,
            InfractionStatus = inf.InfractionStatus,
            InfractionScope = inf.InfractionScope,
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

        return infraction is null
            ? (ControllerEnums.ProfileReturnState.NotFound, null)
            : (ControllerEnums.ProfileReturnState.Ok, infraction);
    }

    public async Task<(ControllerEnums.ProfileReturnState, List<InfractionDto>?)> GetInfractions()
    {
        var infractions = await _context.Infractions.Select(inf => new InfractionDto
        {
            InfractionGuid = inf.InfractionGuid,
            InfractionType = inf.InfractionType,
            InfractionStatus = inf.InfractionStatus,
            InfractionScope = inf.InfractionScope,
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

        infractions = infractions.OrderByDescending(x => x.Submitted).ToList();

        return infractions.Count is 0
            ? (ControllerEnums.ProfileReturnState.Ok, new List<InfractionDto>())
            : (ControllerEnums.ProfileReturnState.Ok, infractions);
    }

    public async Task<int> GetInfractionDayCount()
    {
        return await _context.Infractions
            .Where(x => x.Submitted + TimeSpan.FromDays(1) > DateTimeOffset.UtcNow
                        && x.InfractionType == InfractionType.Ban
                        && x.InfractionScope == InfractionScope.Global
                        && x.InfractionStatus == InfractionStatus.Active)
            .CountAsync();
    }

    public async Task<bool> SubmitEvidence(InfractionDto request)
    {
        var infraction = await _context.Infractions
            .AsTracking()
            .FirstOrDefaultAsync(x => x.InfractionGuid == request.InfractionGuid);

        if (infraction is null) return false;

        infraction.Evidence = request.Evidence;
        _context.Infractions.Update(infraction);
        await _context.SaveChangesAsync();
        return true;
    }
}
