using GlobalInfraction.WebCore.Server.Context;
using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Server.Interfaces;
using GlobalInfraction.WebCore.Server.Models;
using GlobalInfraction.WebCore.Shared.Enums;
using GlobalInfraction.WebCore.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GlobalInfraction.WebCore.Server.Services;

public class EntityService : IEntityService
{
    private readonly DataContext _context;

    public EntityService(DataContext context)
    {
        _context = context;
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
                Infractions = profile.Infractions
                    .Where(inf => inf.Target.Identity == identity)
                    .Select(inf => new InfractionDto
                    {
                        InfractionType = inf.InfractionType,
                        InfractionStatus = inf.InfractionStatus,
                        InfractionScope = inf.InfractionScope,
                        InfractionGuid = inf.InfractionGuid,
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
                            Reputation = inf.Admin.Reputation
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
                Reputation = profile.Reputation
            }).FirstOrDefaultAsync();

        if (entity?.Infractions is null) return entity;

        // Check and expire infractions
        var updatedInfractions = new List<Guid>();
        foreach (var inf in entity.Infractions)
        {
            if (inf.Duration is null) continue;
            if (inf.InfractionStatus != InfractionStatus.Active || !(DateTimeOffset.Now > inf.Submitted + inf.Duration)) continue;

            inf.InfractionStatus = InfractionStatus.Expired;
            updatedInfractions.Add(inf.InfractionGuid);
        }

        if (!updatedInfractions.Any()) return entity;

        var infraction = await _context.Infractions
            .AsTracking()
            .Where(x => updatedInfractions.Contains(x.InfractionGuid))
            .ToListAsync();

        foreach (var inf in infraction)
        {
            inf.InfractionStatus = InfractionStatus.Expired;
            _context.Infractions.Update(inf);
        }

        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<List<EntityDto>> GetUsers()
    {
        var result = await _context.Entities
            .Select(profile => new EntityDto
            {
                Identity = profile.Identity,
                Alias = new AliasDto
                {
                    UserName = profile.CurrentAlias.Alias.UserName,
                    Changed = profile.CurrentAlias.Alias.Changed
                },
                Infractions = profile.Infractions.Select(inf => new InfractionDto
                {
                    InfractionType = inf.InfractionType,
                    InfractionStatus = inf.InfractionStatus,
                    InfractionScope = inf.InfractionScope,
                    InfractionGuid = inf.InfractionGuid,
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
                        Reputation = inf.Admin.Reputation
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
                Reputation = profile.Reputation
            })
            .ToListAsync();

        result = result.OrderByDescending(x => x.HeartBeat).ToList();

        return result;
    }

    public async Task<ControllerEnums.ProfileReturnState> CreateOrUpdate(EntityDto request)
    {
        var user = await _context.Entities
            .AsTracking()
            .Include(x => x.Aliases)
            .SingleOrDefaultAsync(user => user.Identity == request.Identity);

        var efServer = await _context.Servers
            .AsNoTracking()
            .Where(x => x.Instance.InstanceGuid == request.Instance!.InstanceGuid)
            .FirstOrDefaultAsync(y => y.ServerId == request.Server!.ServerId);

        // Update existing user
        if (user is not null)
        {
            // Downside for this, if they change their name to something then back, the current will be the 'old' name
            var existingAlias = await _context.Aliases
                .Where(x => x.EntityId == user.Id)
                .AnyAsync(profile =>
                    profile.UserName == request.Alias!.UserName && profile.IpAddress == request.Alias.IpAddress);

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

                // update this to save to the Alias table instead of the user's nav prop
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
            Reputation = 10,
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

        entity.CurrentAlias = currentAlias;
        _context.CurrentAliases.Add(currentAlias);
        await _context.SaveChangesAsync();

        return ControllerEnums.ProfileReturnState.Created;
    }

    public async Task<bool> HasEntity(string identity) => await _context.Entities.AnyAsync(x => x.Identity == identity);

    public async Task<int> GetEntityCount()
    {
        return await _context.Entities.CountAsync();
    }
}
