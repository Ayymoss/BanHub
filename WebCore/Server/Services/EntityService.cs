using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Models.Context;
using BanHub.WebCore.Shared.DTOs;
using BanHub.WebCore.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BanHub.WebCore.Server.Services;

public class EntityService : IEntityService
{
    private readonly DataContext _context;
    private readonly IStatisticService _statisticService;
    private readonly IDiscordWebhookService _discordWebhook;

    public EntityService(DataContext context, IStatisticService statisticService, IDiscordWebhookService discordWebhook)
    {
        _context = context;
        _statisticService = statisticService;
        _discordWebhook = discordWebhook;
    }

    public async Task<EntityDto?> GetUserAsync(string identity, bool privileged)
    {
        // Find profile
        var entity = await _context.Entities
            .Where(profile => profile.Identity == identity)
            .Select(profile => new EntityDto
            {
                Identity = profile.Identity,
                Alias = new AliasDto
                {
                    UserName = profile.CurrentAlias.Alias.UserName,
                    IpAddress = profile.CurrentAlias.Alias.IpAddress,
                    Changed = profile.CurrentAlias.Alias.Changed
                },
                Notes = profile.Notes
                    .Where(x => x.Target.Identity == identity)
                    .Select(note => new NoteDto
                    {
                        NoteGuid = note.NoteGuid,
                        Created = note.Created,
                        Message = note.Message,
                        IsPrivate = note.IsPrivate,
                        Admin = new EntityDto
                        {
                            Identity = note.Admin.Identity,
                            Alias = new AliasDto
                            {
                                UserName = note.Admin.CurrentAlias.Alias.UserName,
                                IpAddress = note.Admin.CurrentAlias.Alias.IpAddress,
                                Changed = note.Admin.CurrentAlias.Alias.Changed
                            }
                        }
                    }).ToList(),
                Servers = profile.ServerConnections
                    .Where(x => x.Entity.Identity == identity)
                    .OrderBy(x => x.Connected)
                    .Select(server => new ServerDto
                    {
                        ServerId = server.Server.ServerId,
                        ServerName = server.Server.ServerName,
                        ServerIp = server.Server.ServerIp,
                        ServerPort = server.Server.ServerPort,
                        ServerGame = server.Server.ServerGame,
                        Connected = server.Connected,
                        Instance = new InstanceDto
                        {
                            InstanceIp = server.Server.Instance.InstanceIp,
                            InstanceName = server.Server.Instance.InstanceName
                        }
                    }).ToList(),
                Penalties = profile.Penalties
                    .Where(inf => inf.Target.Identity == identity)
                    .Select(inf => new PenaltyDto
                    {
                        PenaltyType = inf.PenaltyType,
                        PenaltyStatus = inf.PenaltyStatus,
                        PenaltyScope = inf.PenaltyScope,
                        PenaltyGuid = inf.PenaltyGuid,
                        Duration = inf.Duration,
                        Reason = inf.Reason,
                        Evidence = inf.Evidence,
                        Submitted = inf.Submitted,
                        Admin = new EntityDto
                        {
                            Identity = inf.Admin.Identity,
                            Alias = new AliasDto
                            {
                                UserName = inf.Admin.CurrentAlias.Alias.UserName,
                                IpAddress = inf.Admin.CurrentAlias.Alias.IpAddress,
                                Changed = inf.Admin.CurrentAlias.Alias.Changed
                            },
                        },
                        Instance = new InstanceDto
                        {
                            InstanceGuid = inf.Instance.InstanceGuid,
                            InstanceIp = inf.Instance.InstanceIp,
                            InstanceName = inf.Instance.InstanceName
                        }
                    }).ToList(),
                HeartBeat = profile.HeartBeat,
                Created = profile.Created,
                TotalConnections = profile.TotalConnections,
                PlayTime = profile.PlayTime,
                HasIdentityBan = false
            }).FirstOrDefaultAsync();

        if (entity is null) return null;

        if (entity.Penalties is null) // TODO: REDUNDANT CODE! :L
        {
            if (privileged) return entity;
            // Strip sensitive data if not privileged
            entity.Alias!.IpAddress = null;
            if (entity.Notes is not null && entity.Notes.Any())
            {
                foreach (var note in entity.Notes.Where(x => x.IsPrivate == true).ToList())
                {
                    entity.Notes.Remove(note);
                }
            }
            else
            {
                entity.Notes = null;
            }

            return entity;
        }

        // Check if user is globally banned
        var hasActiveIdentityBan = await _context.PenaltyIdentifiers
            .AnyAsync(x => (x.IpAddress == entity.Alias!.IpAddress || x.Identity == entity.Identity)
                           && x.Expiration > DateTimeOffset.UtcNow);
        entity.HasIdentityBan = hasActiveIdentityBan;

        // Strip sensitive data if not privileged
        if (!privileged) // TODO: REDUNDANT CODE! :L
        {
            entity.Alias!.IpAddress = null;
            if (entity.Notes is not null && entity.Notes.Any())
            {
                foreach (var note in entity.Notes.Where(x => x.IsPrivate == true).ToList())
                {
                    entity.Notes.Remove(note);
                }
            }
            else
            {
                entity.Notes = null;
            }

            foreach (var penalty in entity.Penalties!)
            {
                penalty.Admin!.Alias!.IpAddress = null;
            }
        }

        // Check and expire infractions
        var updatedPenalty = new List<Guid>();
        foreach (var inf in entity.Penalties)
        {
            if (inf.Duration is null) continue;
            if (inf.PenaltyStatus != PenaltyStatus.Active || !(DateTimeOffset.UtcNow > inf.Submitted + inf.Duration)) continue;

            inf.PenaltyStatus = PenaltyStatus.Expired;
            updatedPenalty.Add(inf.PenaltyGuid);
        }

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

    public async Task<ControllerEnums.ReturnState> CreateOrUpdateAsync(EntityDto request)
    {
        var user = await _context.Entities
            .AsTracking()
            .Include(x => x.Aliases)
            .SingleOrDefaultAsync(user => user.Identity == request.Identity);

        EFServer? efServer = null;
        if (request.Server is not null)
        {
            efServer = await _context.Servers
                .AsNoTracking()
                .Where(x => x.Instance.InstanceGuid == request.Instance!.InstanceGuid)
                .FirstOrDefaultAsync(y => y.ServerId == request.Server.ServerId);
        }

        // Update existing user
        if (user is not null)
        {
            // Downside for this, if they change their name to something then back, the current will be the 'old' name
            // TODO: Maybe use .Last?
            var existingAlias = await _context.Aliases
                .Where(x => x.EntityId == user.Id)
                .AnyAsync(alias => alias.UserName == request.Alias!.UserName && alias.IpAddress == request.Alias.IpAddress);

            if (!existingAlias)
            {
                var mostRecentAlias = await _context.CurrentAliases
                    .AsTracking()
                    .SingleAsync(x => x.EntityId == user.Id);

                var updatedAlias = new EFAlias
                {
                    UserName = request.Alias!.UserName,
                    IpAddress = request.Alias.IpAddress!,
                    Changed = DateTimeOffset.UtcNow,
                    EntityId = user.Id
                };

                user.Aliases.Add(updatedAlias);
                mostRecentAlias.Alias = updatedAlias;
                _context.CurrentAliases.Update(mostRecentAlias);
            }

            if (efServer is not null)
            {
                var server = new EFServerConnection
                {
                    Connected = DateTimeOffset.UtcNow,
                    EntityId = user.Id,
                    ServerId = efServer.Id,
                };
                _context.ServerConnections.Add(server);
            }

            user.HeartBeat = DateTimeOffset.UtcNow;
            user.TotalConnections++;
            _context.Entities.Update(user);
            await _context.SaveChangesAsync();

            return ControllerEnums.ReturnState.Updated;
        }

        // Create the user
        var entity = new EFEntity
        {
            Identity = request.Identity,
            HeartBeat = DateTimeOffset.UtcNow,
            WebRole = WebRole.WebUser,
            InstanceRole = request.InstanceRole!.Value,
            Created = DateTimeOffset.UtcNow,
            PlayTime = TimeSpan.Zero,
            TotalConnections = 1
        };

        var alias = new EFAlias
        {
            Entity = entity,
            UserName = request.Alias!.UserName,
            IpAddress = request.Alias.IpAddress!,
            Changed = DateTimeOffset.UtcNow
        };

        var currentAlias = new EFCurrentAlias
        {
            Alias = alias,
            Entity = entity
        };

        if (efServer is not null)
        {
            var server = new EFServerConnection
            {
                Connected = DateTimeOffset.UtcNow,
                Entity = entity,
                ServerId = efServer.Id
            };
            _context.ServerConnections.Add(server);
        }

        if (user is null)
            await _statisticService.UpdateStatisticAsync(ControllerEnums.StatisticType.EntityCount, ControllerEnums.StatisticTypeAction.Add);

        entity.CurrentAlias = currentAlias;
        _context.CurrentAliases.Add(currentAlias);
        await _context.SaveChangesAsync();

        return ControllerEnums.ReturnState.Created;
    }

    public async Task<bool> HasEntityAsync(string identity) => await _context.Entities.AnyAsync(x => x.Identity == identity);

    public async Task<List<EntityDto>> PaginationAsync(PaginationDto pagination)
    {
        var query = _context.Entities.AsQueryable();

        if (!string.IsNullOrWhiteSpace(pagination.SearchString))
        {
            query = query.Where(search =>
                EF.Functions.ILike(search.Identity, $"%{pagination.SearchString}%") ||
                EF.Functions.ILike(search.CurrentAlias.Alias.UserName, $"%{pagination.SearchString}%"));
        }

        query = pagination.SortLabel switch
        {
            "Id" => query.OrderByDirection((SortDirection)pagination.SortDirection!, entity => entity.Identity),
            "Name" => query.OrderByDirection((SortDirection)pagination.SortDirection!, entity => entity.CurrentAlias.Alias.UserName),
            "Penalty" => query.OrderByDirection((SortDirection)pagination.SortDirection!, entity => entity.Penalties.Count),
            "Online" => query.OrderByDirection((SortDirection)pagination.SortDirection!, entity => entity.HeartBeat),
            "Created" => query.OrderByDirection((SortDirection)pagination.SortDirection!, entity => entity.Created),
            _ => query
        };

        var pagedData = await query
            .Skip(pagination.Page!.Value * pagination.PageSize!.Value)
            .Take(pagination.PageSize.Value)
            .Select(profile => new EntityDto
            {
                Identity = profile.Identity,
                Alias = new AliasDto
                {
                    UserName = profile.CurrentAlias.Alias.UserName,
                    Changed = profile.CurrentAlias.Alias.Changed
                },
                Penalties = profile.Penalties
                    .Select(inf => new PenaltyDto
                    {
                        PenaltyType = inf.PenaltyType,
                        PenaltyStatus = inf.PenaltyStatus,
                        PenaltyScope = inf.PenaltyScope,
                        PenaltyGuid = inf.PenaltyGuid,
                        Duration = inf.Duration,
                        Reason = inf.Reason,
                        Evidence = inf.Evidence,
                        Admin = new EntityDto
                        {
                            Identity = inf.Admin.Identity,
                            Alias = new AliasDto
                            {
                                UserName = inf.Admin.CurrentAlias.Alias.UserName,
                                Changed = inf.Admin.CurrentAlias.Alias.Changed
                            }
                        },
                        Instance = new InstanceDto
                        {
                            InstanceGuid = inf.Instance.InstanceGuid,
                            InstanceIp = inf.Instance.InstanceIp,
                            InstanceName = inf.Instance.InstanceName
                        }
                    }).ToList(),
                HeartBeat = profile.HeartBeat,
                Created = profile.Created,
            }).ToListAsync();

        return pagedData;
    }

    public async Task<string?> GetAuthenticationTokenAsync(EntityDto request)
    {
        var entity = await _context.Entities.FirstOrDefaultAsync(x => x.Identity == request.Identity);
        if (entity is null) return null;

        var hasActiveToken = await _context.AuthTokens.FirstOrDefaultAsync(x =>
            x.EntityId == entity.Id && x.Created + TimeSpan.FromMinutes(5) > DateTimeOffset.UtcNow && !x.Used);
        if (hasActiveToken is not null) return hasActiveToken.Token;

        const string characters = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ23456789";
        var result = string.Empty;
        for (var i = 0; i < 6; i++)
        {
            result += characters[Random.Shared.Next(characters.Length)];
        }

        var token = new EFAuthToken
        {
            Token = result,
            Created = DateTimeOffset.UtcNow,
            EntityId = entity.Id,
            Used = false
        };

        _context.AuthTokens.Add(token);
        await _context.SaveChangesAsync();
        return result;
    }

    public async Task<bool> AddNoteAsync(NoteDto request, string adminIdentity)
    {
        var admin = await _context.Entities.FirstOrDefaultAsync(x => x.Identity == adminIdentity);
        var target = await _context.Entities.FirstOrDefaultAsync(x => x.Identity == request.Target!.Identity);
        if (admin is null || target is null) return false;

        var note = new EFNote
        {
            NoteGuid = request.NoteGuid ?? Guid.NewGuid(),
            TargetId = target.Id,
            AdminId = admin.Id,
            Message = request.Message!,
            IsPrivate = request.IsPrivate!.Value,
            Created = DateTimeOffset.UtcNow,
        };

        _context.Notes.Add(note);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveNoteAsync(NoteDto request, string requestingAdmin)
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
