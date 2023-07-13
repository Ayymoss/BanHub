using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Models.Domains;
using BanHub.WebCore.Shared.Models;
using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Domains;
using BanHubData.Enums;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BanHub.WebCore.Server.Services;

public class PlayerService : IPlayerService
{
    private readonly DataContext _context;
    private readonly IDiscordWebhookService _discordWebhook;

    public PlayerService(DataContext context, IDiscordWebhookService discordWebhook)
    {
        _context = context;
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
                    .Select(server => new BanHubData.Domains.Server
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
    
}
