using BanHub.WebCore.Server.Context;
using Data.Enums;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Models;
using BanHub.WebCore.Server.Models.Context;
using Data.Commands;
using Data.Commands.Player;
using Data.Domains;
using Data.Enums;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BanHub.WebCore.Server.Services;

public class PenaltyService : IPenaltyService
{
    private readonly IDiscordWebhookService _discordWebhook;
    private readonly IStatisticService _statisticService;
    private readonly ILogger _logger;
    private readonly DataContext _context;
    private readonly IPlayerService _playerService;

    public PenaltyService(DataContext context, IPlayerService playerService, IDiscordWebhookService discordWebhook,
        IStatisticService statisticService, ILogger<PenaltyService> logger)
    {
        _discordWebhook = discordWebhook;
        _statisticService = statisticService;
        _logger = logger;
        _context = context;
        _playerService = playerService;
    }
    
    public async Task<List<Penalty>> PaginationAsync(Pagination pagination)
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
            "Id" => query.OrderByDirection((SortDirection)pagination.SortDirection, key => key.PenaltyGuid),
            "Target Name" => query.OrderByDirection((SortDirection)pagination.SortDirection,
                key => key.Admin.CurrentAlias.Alias.UserName),
            "Admin Name" => query.OrderByDirection((SortDirection)pagination.SortDirection,
                key => key.Target.Penalties.Count),
            "Reason" => query.OrderByDirection((SortDirection)pagination.SortDirection, key => key.Reason),
            "Type" => query.OrderByDirection((SortDirection)pagination.SortDirection, key => key.PenaltyType),
            "Status" => query.OrderByDirection((SortDirection)pagination.SortDirection, key => key.PenaltyStatus),
            "Scope" => query.OrderByDirection((SortDirection)pagination.SortDirection, key => key.PenaltyScope),
            "Instance" => query.OrderByDirection((SortDirection)pagination.SortDirection,
                key => key.Instance.InstanceName),
            "Submitted" => query.OrderByDirection((SortDirection)pagination.SortDirection, key => key.Submitted),
            _ => query
        };

        var pagedData = await query
            .Skip(pagination.Page * pagination.PageSize)
            .Take(pagination.PageSize)
            .Where(x => x.PenaltyType == PenaltyType.Ban
                        || x.PenaltyType == PenaltyType.TempBan
                        || x.PenaltyType == PenaltyType.Kick
                        || x.PenaltyType == PenaltyType.Unban)
            .Select(penalty => new Penalty // TODO: this needs to be a View Model
            {
                PenaltyGuid = penalty.PenaltyGuid,
                PenaltyType = penalty.PenaltyType,
                PenaltyStatus = penalty.PenaltyStatus,
                PenaltyScope = penalty.PenaltyScope,
                Submitted = penalty.Submitted,
                AdminIdentity = penalty.Admin.Identity,
                Reason = penalty.Reason,
                Evidence = penalty.Evidence,
                Duration = penalty.Duration,
                InstanceGuid = penalty.Instance.InstanceGuid,
                TargetIdentity = penalty.Target.Identity
            }).ToListAsync();

        return pagedData;
    }

    public async Task<List<Penalty>> GetLatestThreeBansAsync()
    {
        var bans = await _context.Penalties
            .Where(x => x.PenaltyType == PenaltyType.Ban
                        && x.PenaltyStatus == PenaltyStatus.Active
                        && x.PenaltyScope == PenaltyScope.Global
                        && x.Submitted > DateTimeOffset.UtcNow.AddMonths(-1)) // Arbitrary time frame. We don't care about anything too old.
            .OrderByDescending(x => x.Id)
            .Take(3)
            .Select(penalty => new Penalty
            {
                PenaltyGuid = penalty.PenaltyGuid,
                PenaltyType = penalty.PenaltyType,
                PenaltyStatus = penalty.PenaltyStatus,
                PenaltyScope = penalty.PenaltyScope,
                Submitted = penalty.Submitted,
                Admin = new Player
                {
                    Identity = penalty.Admin.Identity,
                    Alias = new Alias
                    {
                        UserName = penalty.Admin.CurrentAlias.Alias.UserName,
                    }
                },
                Reason = penalty.Reason,
                Evidence = penalty.Evidence,
                Duration = penalty.Duration,
                Instance = new Instance
                {
                    InstanceName = penalty.Instance.InstanceName,
                    InstanceGuid = penalty.Instance.InstanceGuid
                },
                Target = new Player
                {
                    Identity = penalty.Target.Identity,
                    Alias = new Alias
                    {
                        UserName = penalty.Target.CurrentAlias.Alias.UserName,
                    }
                }
            }).ToListAsync();

        return bans;
    }

    public async Task<bool> RemovePenaltyAsync(Penalty request, string requestingAdmin)
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
        await _statisticService.UpdateStatisticAsync(ControllerEnums.StatisticType.PenaltyCount,
            ControllerEnums.StatisticTypeAction.Remove);
        await _discordWebhook.CreateAdminActionHookAsync("Penalty Deletion!", message);
        return true;
    }

    public async Task<(ControllerEnums.ReturnState, List<Penalty>?)> GetPenaltiesAsync(string? penaltyGuid = null)
    {
        Guid? guid = null;

        if (penaltyGuid != null)
        {
            if (!Guid.TryParse(penaltyGuid, out var parsedGuid)) return (ControllerEnums.ReturnState.BadRequest, null);
            guid = parsedGuid;
        }

        var penaltiesQuery = _context.Penalties.Select(inf => new Penalty
        {
            PenaltyGuid = inf.PenaltyGuid,
            PenaltyType = inf.PenaltyType,
            PenaltyStatus = inf.PenaltyStatus,
            PenaltyScope = inf.PenaltyScope,
            Submitted = inf.Submitted,
            Admin = new Player
            {
                Identity = inf.Admin.Identity,
                Alias = new Alias
                {
                    UserName = inf.Admin.CurrentAlias.Alias.UserName,
                }
            },
            Reason = inf.Reason,
            Evidence = inf.Evidence,
            Duration = inf.Duration,
            Instance = new Instance
            {
                InstanceName = inf.Instance.InstanceName,
                InstanceGuid = inf.Instance.InstanceGuid
            },
            Target = new Player
            {
                Identity = inf.Target.Identity,
                Alias = new Alias
                {
                    UserName = inf.Target.CurrentAlias.Alias.UserName,
                }
            }
        });

        if (guid != null)
        {
            var penalty = await penaltiesQuery.FirstOrDefaultAsync(inf => inf.PenaltyGuid == guid);
            return penalty == null
                ? (ControllerEnums.ReturnState.NotFound, null)
                : (ControllerEnums.ReturnState.Ok, new List<Penalty> {penalty});
        }

        var penalties = await penaltiesQuery.OrderByDescending(x => x.Submitted).ToListAsync();
        return (ControllerEnums.ReturnState.Ok, penalties);
    }
}
