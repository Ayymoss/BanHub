using GlobalInfraction.WebCore.Server.Context;
using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Server.Interfaces;
using GlobalInfraction.WebCore.Server.Models;
using GlobalInfraction.WebCore.Shared.Enums;
using GlobalInfraction.WebCore.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlobalInfraction.WebCore.Server.Services;

public class InfractionService : IInfractionService
{
    private readonly IDiscordWebhookService _discordWebhook;
    private readonly DataContext _context;
    private readonly IEntityService _entityService;

    public InfractionService(DataContext context, IEntityService entityService, IDiscordWebhookService discordWebhook)
    {
        _discordWebhook = discordWebhook;
        _context = context;
        _entityService = entityService;
    }

    public async Task<(ControllerEnums.ProfileReturnState, Guid?)> AddInfraction([FromBody] InfractionDto request)
    {
        var user = await _context.Entities
            .AsTracking()
            .Include(x => x.CurrentAlias.Alias)
            .FirstOrDefaultAsync(entity => entity.Identity == request.Target!.Identity);

        var admin = await _context.Entities
            .FirstOrDefaultAsync(profile => profile.Identity == request.Admin!.Identity);

        if (user is null)
        {
            await _entityService.CreateOrUpdate(request.Target!);
            user = await _context.Entities
                .AsTracking()
                .Include(x => x.CurrentAlias.Alias)
                .FirstOrDefaultAsync(entity => entity.Identity == request.Target!.Identity);
        }

        if (user is null || admin is null) return (ControllerEnums.ProfileReturnState.NotFound, null);

        // Check if the user has an existing ban and the incoming is an unban from the server that banned them
        var infraction = await _context.Infractions
            .AsTracking()
            .Where(inf => inf.TargetId == user.Id && inf.Instance.InstanceGuid == request.Instance!.InstanceGuid)
            .FirstOrDefaultAsync(inf =>
                (inf.InfractionType == InfractionType.Ban || inf.InfractionType == InfractionType.TempBan) &&
                inf.InfractionStatus == InfractionStatus.Active);

        if (infraction is not null)
        {
            switch (request.InfractionType)
            {
                // If the incoming request is an unban, unban them.
                case InfractionType.Unban:
                    infraction.InfractionStatus = InfractionStatus.Revoked;
                    _context.Update(infraction);
                    break;
                // If they're already banned. We don't need to keep creating kick infractions.
                case InfractionType.Kick:
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
        
        var statistic = await _context.Statistics.FirstAsync(x => x.Id == (int)ControllerEnums.StatisticType.InfractionCount);
        statistic.Count++;
        _context.Statistics.Update(statistic);
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
            ? (ControllerEnums.ProfileReturnState.NotFound, null)
            : (ControllerEnums.ProfileReturnState.Ok, infractions);
    }

    public async Task<int> GetInfractionCount()
    {
        return await _context.Infractions.CountAsync();
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
