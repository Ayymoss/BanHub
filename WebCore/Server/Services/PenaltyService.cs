using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Models;
using BanHub.WebCore.Server.Models.Context;
using BanHub.WebCore.Shared.DTOs;
using BanHub.WebCore.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

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
            return (ControllerEnums.ProfileReturnState.NoContent, null);

        // If any penalties check and action
        var penalty = await _context.Penalties
            .AsTracking()
            .Where(inf =>
                inf.TargetId == user.Id && inf.Instance.InstanceGuid == request.Instance!.InstanceGuid
                                        && (inf.PenaltyType == PenaltyType.Ban || inf.PenaltyType == PenaltyType.TempBan)
                                        && inf.PenaltyStatus == PenaltyStatus.Active)
            .ToListAsync();

        if (penalty.Any())
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
                    return (ControllerEnums.ProfileReturnState.NoContent, null);
            }
        }
        else
        {
            switch (request.PenaltyType)
            {
                // Trying to unban when no record of a ban exists.
                case PenaltyType.Unban:
                    return (ControllerEnums.ProfileReturnState.NoContent, null);
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

        if (request is {PenaltyType: PenaltyType.Ban, PenaltyScope: PenaltyScope.Global})
        {
            var identifier = new EFPenaltyIdentifier
            {
                Identity = user.Identity,
                IpAddress = user.CurrentAlias.Alias.IpAddress,
                Expiration = DateTimeOffset.UtcNow.AddMonths(1),
                Penalty = penaltyModel,
                EntityId = user.Id
            };
            _context.Add(identifier);
        }

        await _statisticService.UpdateDayStatistic(new StatisticBan
        {
            BanGuid = penaltyModel.PenaltyGuid,
            Submitted = DateTimeOffset.UtcNow
        });

        await _statisticService.UpdateStatistic(ControllerEnums.StatisticType.PenaltyCount, ControllerEnums.StatisticTypeAction.Add);
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

    public async Task<List<PenaltyDto>> Pagination(PaginationDto pagination)
    {
        var query = _context.Penalties.AsQueryable();

        if (!string.IsNullOrWhiteSpace(pagination.SearchString))
        {
            query = query.Where(search =>
                EF.Functions.ILike(search.PenaltyGuid.ToString(), $"%{pagination.SearchString}%") ||
                EF.Functions.ILike(search.Admin.CurrentAlias.Alias.UserName, $"%{pagination.SearchString}%") ||
                EF.Functions.ILike(search.Target.CurrentAlias.Alias.UserName, $"%{pagination.SearchString}%"));
        }

        query = pagination.SortLabel switch
        {
            "Id" => query.OrderByDirection((SortDirection)pagination.SortDirection!, key => key.PenaltyGuid),
            "Target Name" => query.OrderByDirection((SortDirection)pagination.SortDirection!, key => key.Admin.CurrentAlias.Alias.UserName),
            "Admin Name" => query.OrderByDirection((SortDirection)pagination.SortDirection!, key => key.Target.Penalties.Count),
            "Reason" => query.OrderByDirection((SortDirection)pagination.SortDirection!, key => key.Reason),
            "Type" => query.OrderByDirection((SortDirection)pagination.SortDirection!, key => key.PenaltyType),
            "Status" => query.OrderByDirection((SortDirection)pagination.SortDirection!, key => key.PenaltyStatus),
            "Scope" => query.OrderByDirection((SortDirection)pagination.SortDirection!, key => key.PenaltyScope),
            "Instance" => query.OrderByDirection((SortDirection)pagination.SortDirection!, key => key.Instance.InstanceName),
            "Submitted" => query.OrderByDirection((SortDirection)pagination.SortDirection!, key => key.Submitted),
            _ => query
        };

        var pagedData = await query
            .Skip(pagination.Page!.Value * pagination.PageSize!.Value)
            .Take(pagination.PageSize.Value)
            .Where(x => x.PenaltyType == PenaltyType.Ban
                        || x.PenaltyType == PenaltyType.TempBan
                        || x.PenaltyType == PenaltyType.Kick
                        || x.PenaltyType == PenaltyType.Unban)
            .Select(penalty => new PenaltyDto
            {
                PenaltyGuid = penalty.PenaltyGuid,
                PenaltyType = penalty.PenaltyType,
                PenaltyStatus = penalty.PenaltyStatus,
                PenaltyScope = penalty.PenaltyScope,
                Submitted = penalty.Submitted,
                Admin = new EntityDto
                {
                    Identity = penalty.Admin.Identity,
                    Alias = new AliasDto
                    {
                        UserName = penalty.Admin.CurrentAlias.Alias.UserName,
                    }
                },
                Reason = penalty.Reason,
                Evidence = penalty.Evidence,
                Duration = penalty.Duration,
                Instance = new InstanceDto
                {
                    InstanceName = penalty.Instance.InstanceName,
                    InstanceGuid = penalty.Instance.InstanceGuid
                },
                Target = new EntityDto
                {
                    Identity = penalty.Target.Identity,
                    Alias = new AliasDto
                    {
                        UserName = penalty.Target.CurrentAlias.Alias.UserName,
                    }
                }
            }).ToListAsync();

        return pagedData;
    }

    public async Task<List<PenaltyDto>> GetLatestThreeBans()
    {
        var bans = await _context.Penalties
            .Where(x => x.PenaltyType == PenaltyType.Ban
                        && x.PenaltyStatus == PenaltyStatus.Active
                        && x.PenaltyScope == PenaltyScope.Global
                        && x.Submitted > DateTimeOffset.UtcNow.AddMonths(-1)) // Arbitrary time frame. We don't care about anything too old.
            .OrderByDescending(x => x.Id)
            .Take(3)
            .Select(penalty => new PenaltyDto
            {
                PenaltyGuid = penalty.PenaltyGuid,
                PenaltyType = penalty.PenaltyType,
                PenaltyStatus = penalty.PenaltyStatus,
                PenaltyScope = penalty.PenaltyScope,
                Submitted = penalty.Submitted,
                Admin = new EntityDto
                {
                    Identity = penalty.Admin.Identity,
                    Alias = new AliasDto
                    {
                        UserName = penalty.Admin.CurrentAlias.Alias.UserName,
                    }
                },
                Reason = penalty.Reason,
                Evidence = penalty.Evidence,
                Duration = penalty.Duration,
                Instance = new InstanceDto
                {
                    InstanceName = penalty.Instance.InstanceName,
                    InstanceGuid = penalty.Instance.InstanceGuid
                },
                Target = new EntityDto
                {
                    Identity = penalty.Target.Identity,
                    Alias = new AliasDto
                    {
                        UserName = penalty.Target.CurrentAlias.Alias.UserName,
                    }
                }
            }).ToListAsync();

        return bans;
    }

    public async Task<bool> RemovePenalty(PenaltyDto request, string requestingAdmin)
    {
        var penalty = await _context.Penalties.FirstOrDefaultAsync(x => x.PenaltyGuid == request.PenaltyGuid);
        if (penalty is null) return false;

        var penaltyInfo = await _context.Penalties
            .Where(x => x.PenaltyGuid == penalty.PenaltyGuid)
            .Select(x => new
            {
                AdminIdentity = x.Admin.Identity,
                TargetIdentity = x.Target.Identity,
                x.PenaltyGuid,
                x.Reason,
                x.Evidence
            }).FirstOrDefaultAsync();

        var message = penaltyInfo is null
            ? $"Penalty **{penalty.PenaltyGuid}** was deleted by **{requestingAdmin}** but no information could be found."
            : $"**Penalty**: {penaltyInfo.PenaltyGuid}\n" +
              $"**Admin**: {penaltyInfo.AdminIdentity}\n" +
              $"**Target**: {penaltyInfo.TargetIdentity}\n" +
              $"**Reason**: {penaltyInfo.Reason}\n" +
              $"**Evidence**: {penaltyInfo.Evidence ?? "None"}\n\n" +
              $"**Deleted By**: {requestingAdmin}\n" +
              $"**Deleted For**: {request.DeletionReason}";

        _context.Penalties.Remove(penalty);
        await _context.SaveChangesAsync();
        await _statisticService.UpdateStatistic(ControllerEnums.StatisticType.PenaltyCount, ControllerEnums.StatisticTypeAction.Remove);
        await _discordWebhook.CreateAdminActionHook(message);
        return true;
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

    public async Task<bool> SubmitEvidence(PenaltyDto request)
    {
        var penalty = await _context.Penalties
            .AsTracking()
            .FirstOrDefaultAsync(x => x.PenaltyGuid == request.PenaltyGuid);

        if (penalty is null) return false;
        if (penalty.Evidence is not null) return false;

        penalty.Evidence = request.Evidence;
        _context.Penalties.Update(penalty);
        await _context.SaveChangesAsync();
        return true;
    }
}
