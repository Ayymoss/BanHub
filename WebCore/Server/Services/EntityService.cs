using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Models;
using BanHub.WebCore.Shared.DTOs;
using BanHub.WebCore.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BanHub.WebCore.Server.Services;

public class EntityService : IEntityService
{
    private readonly DataContext _context;
    private readonly IStatisticService _statisticService;

    public EntityService(DataContext context, IStatisticService statisticService)
    {
        _context = context;
        _statisticService = statisticService;
    }

    public async Task<EntityDto?> GetUser(string identity)
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
                    Changed = profile.CurrentAlias.Alias.Changed
                },
                Notes = profile.Notes
                    .Where(x => x.Target.Identity == identity)
                    .Select(note => new NoteDto
                    {
                        Id = note.Id,
                        Created = note.Created,
                        Message = note.Message,
                        Admin = new EntityDto
                        {
                            Identity = note.Admin.Identity,
                            Alias = new AliasDto
                            {
                                UserName = note.Admin.CurrentAlias.Alias.UserName,
                                Changed = note.Admin.CurrentAlias.Alias.Changed
                            },
                        }
                    }).ToList(),
                Servers = profile.ServerConnections
                    .Where(x => x.Entity.Identity == identity)
                    .Select(server => new ServerDto
                    {
                        ServerId = server.Server.ServerId,
                        ServerName = server.Server.ServerName,
                        ServerIp = server.Server.ServerIp,
                        ServerPort = server.Server.ServerPort,
                        Connected = server.Connected
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
                Created = profile.Created
            }).FirstOrDefaultAsync();

        if (entity?.Penalties is null) return entity;

        // Check and expire infractions
        var updatedPenalty = new List<Guid>();
        foreach (var inf in entity.Penalties)
        {
            if (inf.Duration is null) continue;
            if (inf.PenaltyStatus != PenaltyStatus.Active || !(DateTimeOffset.Now > inf.Submitted + inf.Duration)) continue;

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

    public async Task<ControllerEnums.ProfileReturnState> CreateOrUpdate(EntityDto request)
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

            var activeBanCount = await _context.Penalties
                .AsNoTracking()
                .Where(inf => inf.PenaltyType == PenaltyType.Ban && inf.PenaltyStatus == PenaltyStatus.Active)
                .CountAsync();

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
            _context.Entities.Update(user);
            await _context.SaveChangesAsync();

            return ControllerEnums.ProfileReturnState.Updated;
        }

        // Create the user
        var entity = new EFEntity
        {
            Identity = request.Identity,
            HeartBeat = DateTimeOffset.UtcNow,
            WebRole = WebRole.User,
            Created = DateTimeOffset.UtcNow
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

        if (user is null) await _statisticService.UpdateStatistic(ControllerEnums.StatisticType.EntityCount);

        entity.CurrentAlias = currentAlias;
        _context.CurrentAliases.Add(currentAlias);
        await _context.SaveChangesAsync();

        return ControllerEnums.ProfileReturnState.Created;
    }

    public async Task<bool> HasEntity(string identity) => await _context.Entities.AnyAsync(x => x.Identity == identity);

    public async Task<int> GetOnlineCount()
    {
        return await _context.Entities
            .Where(x => x.HeartBeat + TimeSpan.FromMinutes(5) > DateTimeOffset.UtcNow)
            .CountAsync();
    }

    public async Task<List<EntityDto>> Pagination(PaginationDto pagination)
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
            })
            .ToListAsync();

        return pagedData;
    }

    public async Task<string?> GetAuthenticationToken(EntityDto request)
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
}
