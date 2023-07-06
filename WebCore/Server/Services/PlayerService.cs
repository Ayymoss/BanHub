using System.Text;
using BanHub.WebCore.Server.Context;
using Data.Enums;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Models.Context;
using Data.Commands;
using Data.Commands.Player;
using Data.Domains;
using Data.Enums;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BanHub.WebCore.Server.Services;

public class PlayerService : IPlayerService
{
    private readonly DataContext _context;
    private readonly IStatisticService _statisticService;
    private readonly IDiscordWebhookService _discordWebhook;

    public PlayerService(DataContext context, IStatisticService statisticService, IDiscordWebhookService discordWebhook)
    {
        _context = context;
        _statisticService = statisticService;
        _discordWebhook = discordWebhook;
    }

    public async Task<Player?> GetUserAsync(string identity, bool privileged)
    {
        var entity = await _context.Players
            .Where(profile => profile.Identity == identity)
            .Select(profile => new Player
            {
                Identity = profile.Identity,
                Alias = new Alias
                {
                    UserName = profile.CurrentAlias.Alias.UserName,
                    IpAddress = privileged ? profile.CurrentAlias.Alias.IpAddress : null,
                    Changed = profile.CurrentAlias.Alias.Changed
                },
                Notes = profile.Notes
                    .Where(x => x.Target.Identity == identity && (!x.IsPrivate || privileged))
                    .Select(note => new Note
                    {
                        NoteGuid = note.NoteGuid,
                        Created = note.Created,
                        Message = note.Message,
                        IsPrivate = note.IsPrivate,
                        Admin = new Player
                        {
                            Identity = note.Admin.Identity,
                            Alias = new Alias
                            {
                                UserName = note.Admin.CurrentAlias.Alias.UserName,
                                IpAddress = privileged ? note.Admin.CurrentAlias.Alias.IpAddress : null,
                                Changed = note.Admin.CurrentAlias.Alias.Changed
                            }
                        }
                    }).ToList(),
                Servers = profile.ServerConnections
                    .Where(x => x.Player.Identity == identity)
                    .OrderBy(x => x.Connected)
                    .Select(server => new Data.Domains.Server
                    {
                        ServerId = server.Server.ServerId,
                        ServerName = server.Server.ServerName,
                        ServerIp = server.Server.ServerIp,
                        ServerPort = server.Server.ServerPort,
                        ServerGame = server.Server.ServerGame,
                        Connected = server.Connected,
                        Instance = new Instance
                        {
                            InstanceIp = server.Server.Instance.InstanceIp,
                            InstanceName = server.Server.Instance.InstanceName
                        }
                    }).ToList(),
                Penalties = profile.Penalties
                    .Where(inf => inf.Target.Identity == identity)
                    .Select(inf => new Penalty
                    {
                        PenaltyType = inf.PenaltyType,
                        PenaltyStatus = inf.PenaltyStatus,
                        PenaltyScope = inf.PenaltyScope,
                        PenaltyGuid = inf.PenaltyGuid,
                        Duration = inf.Duration,
                        Reason = inf.Reason,
                        Evidence = inf.Evidence,
                        Submitted = inf.Submitted,
                        AdminIdentity = inf.Admin.Identity,
                        InstanceGuid = inf.Instance.InstanceGuid
                    }).ToList(),
                HeartBeat = profile.HeartBeat,
                Created = profile.Created,
                TotalConnections = profile.TotalConnections,
                PlayTime = profile.PlayTime,
                HasIdentityBan = false
            }).FirstOrDefaultAsync();

        if (entity is null) return null;

        // Check if user is globally banned
        var hasActiveIdentityBan = await _context.PenaltyIdentifiers
            .AnyAsync(x => (x.IpAddress == entity.Alias.IpAddress || x.Identity == entity.Identity)
                           && x.Expiration > DateTimeOffset.UtcNow);
        entity.HasIdentityBan = hasActiveIdentityBan;

        // Check and expire infractions
        var updatedPenalty = entity.Penalties
            .Where(inf => inf is {Duration: not null, PenaltyStatus: PenaltyStatus.Active} &&
                          DateTimeOffset.UtcNow > inf.Submitted + inf.Duration)
            .Select(inf =>
            {
                inf.PenaltyStatus = PenaltyStatus.Expired;
                return inf.PenaltyGuid;
            }).ToList();

        if (!updatedPenalty.Any()) return entity;

        var penalties = await _context.Penalties
            .AsTracking()
            .Where(x => updatedPenalty.Contains(x.PenaltyGuid))
            .ToListAsync();

        foreach (var penalty in penalties)
        {
            penalty.PenaltyStatus = PenaltyStatus.Expired;
            _context.Penalties.Update(penalty);
        }

        await _context.SaveChangesAsync();

        return entity;
    }
    
    public async Task<bool> HasEntityAsync(string identity) => await _context.Players.AnyAsync(x => x.Identity == identity);

    public async Task<List<Player>> PaginationAsync(Pagination pagination)
    {
        var query = _context.Players.AsQueryable();

        if (!string.IsNullOrWhiteSpace(pagination.SearchString))
        {
            query = query.Where(search =>
                EF.Functions.ILike(search.Identity, $"%{pagination.SearchString}%") ||
                EF.Functions.ILike(search.CurrentAlias.Alias.UserName, $"%{pagination.SearchString}%"));
        }

        query = pagination.SortLabel switch
        {
            "Id" => query.OrderByDirection((SortDirection)pagination.SortDirection, entity => entity.Identity),
            "Name" => query.OrderByDirection((SortDirection)pagination.SortDirection, entity => entity.CurrentAlias.Alias.UserName),
            "Penalty" => query.OrderByDirection((SortDirection)pagination.SortDirection, entity => entity.Penalties.Count),
            "Online" => query.OrderByDirection((SortDirection)pagination.SortDirection, entity => entity.HeartBeat),
            "Created" => query.OrderByDirection((SortDirection)pagination.SortDirection, entity => entity.Created),
            _ => query
        };

        var pagedData = await query
            .Skip(pagination.Page * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(profile => new Player
            {
                Identity = profile.Identity,
                Alias = new Alias
                {
                    UserName = profile.CurrentAlias.Alias.UserName,
                    Changed = profile.CurrentAlias.Alias.Changed
                },
                Penalties = profile.Penalties
                    .Select(inf => new Penalty
                    {
                        PenaltyType = inf.PenaltyType,
                        PenaltyStatus = inf.PenaltyStatus,
                        PenaltyScope = inf.PenaltyScope,
                        PenaltyGuid = inf.PenaltyGuid,
                        Duration = inf.Duration,
                        Reason = inf.Reason,
                        Evidence = inf.Evidence,
                        AdminIdentity = inf.Admin.Identity,
                        InstanceGuid = inf.Instance.InstanceGuid
                    }).ToList(),
                HeartBeat = profile.HeartBeat,
                Created = profile.Created,
            }).ToListAsync();

        return pagedData;
    }

    public async Task<string?> GetAuthenticationTokenAsync(string identity)
    {
        
    }

    public async Task<bool> AddNoteAsync(Note request, string adminIdentity)
    {
        var admin = await _context.Players.FirstOrDefaultAsync(x => x.Identity == adminIdentity);
        var target = await _context.Players.FirstOrDefaultAsync(x => x.Identity == request.Target.Identity);
        if (admin is null || target is null) return false;

        var note = new EFNote
        {
            NoteGuid = request.NoteGuid ?? Guid.NewGuid(),
            TargetId = target.Id,
            AdminId = admin.Id,
            Message = request.Message,
            IsPrivate = request.IsPrivate,
            Created = DateTimeOffset.UtcNow
        };

        _context.Notes.Add(note);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveNoteAsync(Note request, string requestingAdmin)
    {
        var note = await _context.Notes.FirstOrDefaultAsync(x => x.NoteGuid == request.NoteGuid);
        if (note is null) return false;

        var noteInfo = await _context.Notes
            .Where(x => x.NoteGuid == note.NoteGuid)
            .Select(x => new
            {
                x.NoteGuid,
                AdminIdentity = x.Admin.Identity,
                TargetIdentity = x.Target.Identity,
                x.Message,
                x.IsPrivate,
            }).FirstOrDefaultAsync();

        var message = noteInfo is null
            ? $"Penalty **{note.NoteGuid}** was deleted by **{requestingAdmin}** but no information could be found."
            : $"**Penalty**: {noteInfo.NoteGuid}\n" +
              $"**Admin**: {noteInfo.AdminIdentity}\n" +
              $"**Target**: {noteInfo.TargetIdentity}\n" +
              $"**Note**: {noteInfo.Message}\n" +
              $"**Was Public?**: {(!noteInfo.IsPrivate ? "Yes" : "No")}\n\n" +
              $"**Deleted By**: {requestingAdmin}\n" +
              $"**Deleted For**: {request.DeletionReason}";

        _context.Notes.Remove(note);
        await _context.SaveChangesAsync();
        await _discordWebhook.CreateAdminActionHookAsync("Note Deletion!", message);
        return true;
    }
}
