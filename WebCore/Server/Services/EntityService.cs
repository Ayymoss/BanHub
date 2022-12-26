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
    private readonly SqliteDataContext _context;

    public EntityService(SqliteDataContext context)
    {
        _context = context;
    }

    public async Task<EntityDto?> GetUser(string identity)
    {
        // Find profile
        var entity = await _context.Entities.Where(profile => profile.Identity == identity)
            .Select(profile => new
            {
                profile.Id,
                ProfileIdentity = identity,
                profile.Reputation,
                profile.CurrentAlias.Alias
            }).FirstOrDefaultAsync();

        // Return null if not found
        if (entity is null) return null;

        // Get associated infractions
        var infractions = await _context.Infractions.Where(infraction => infraction.Target.Identity == identity)
            .Select(infraction => new InfractionDto
            {
                InfractionType = infraction.InfractionType,
                InfractionScope = infraction.InfractionScope,
                InfractionGuid = infraction.InfractionGuid,
                Admin = new EntityDto
                {
                    Identity = infraction.Admin.Identity,
                    Alias = new AliasDto
                    {
                        UserName = infraction.Admin.CurrentAlias.Alias.UserName,
                        IpAddress = infraction.Admin.CurrentAlias.Alias.IpAddress,
                        Changed = infraction.Admin.CurrentAlias.Alias.Changed
                    },
                    Reputation = infraction.Admin.Reputation
                },
                Reason = infraction.Reason,
                Evidence = infraction.Evidence,
            })
            .ToListAsync();

        // Return Dto
        return new EntityDto
        {
            Identity = entity.ProfileIdentity,
            Reputation = entity.Reputation,
            Alias = new AliasDto
            {
                UserName = entity.Alias.UserName,
                IpAddress = entity.Alias.IpAddress,
                Changed = entity.Alias.Changed
            },
            Infractions = infractions
        };
    }

    // TODO: Implement authorisation.
    public async Task<List<EntityDto>> GetUsers()
    {
        var result = await _context.Entities
            .Select(profile => new EntityDto
            {
                Identity = profile.Identity,
                Alias = new AliasDto
                {
                    UserName = profile.CurrentAlias.Alias.UserName,
                    IpAddress = profile.CurrentAlias.Alias.IpAddress, //TODO: THIS NEEDS TO BE PRIVILEGED
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
                            IpAddress = inf.Admin.CurrentAlias.Alias.IpAddress,
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
                Reputation = profile.Reputation
            })
            .ToListAsync();

        return result;
    }

    public async Task<ControllerEnums.ProfileReturnState> CreateOrUpdate(EntityDto request)
    {
        // todo: fluent validation on request
        var user = await _context.Entities
            .AsTracking()
            .Include(x => x.Aliases)
            .SingleOrDefaultAsync(user => user.Identity == request.Identity);

        // TODO: This check needs to be changed to reply if the instance doesn't have permission to upload
        // TODO: Custom service should be done. Needs testing
        // Update existing user
        if (user is not null)
        {
            // Downside for this, if they change their name to something then back, the current will be the 'old' name
            var existingAlias = await _context.Aliases
                .Where(x => x.EntityId == user.Id)
                .AnyAsync(profile =>
                    profile.UserName == request.Alias.UserName && profile.IpAddress == request.Alias.IpAddress);

            if (!existingAlias)
            {
                var mostRecentAlias = await _context.CurrentAliases
                    .AsTracking()
                    .SingleAsync(x => x.EntityId == user.Id);

                var updatedAlias = new EFAlias
                {
                    UserName = request.Alias.UserName,
                    IpAddress = request.Alias.IpAddress,
                    Changed = DateTimeOffset.UtcNow,
                    EntityId = user.Id
                };

                // update this to save to the Alias table instead of the user's nav prop
                user.Aliases.Add(updatedAlias);
                mostRecentAlias.AliasId = updatedAlias.Id;
                _context.CurrentAliases.Update(mostRecentAlias);
            }

            user.HeartBeat = DateTimeOffset.UtcNow;
            _context.Entities.Update(user);
            await _context.SaveChangesAsync();

            return ControllerEnums.ProfileReturnState.Updated;
        }

        // Create the user
        var alias = new EFAlias
        {
            UserName = request.Alias.UserName,
            IpAddress = request.Alias.IpAddress,
            Changed = DateTimeOffset.UtcNow
        };

        var newProfile = new EFEntity
        {
            Identity = request.Identity,
            Reputation = 10,
            Aliases = new List<EFAlias>
            {
                alias
            },
            HeartBeat = DateTimeOffset.UtcNow,
            WebRole = WebRole.User
        };

        var currentAlias = new EFCurrentAlias
        {
            AliasId = alias.Id,
            EntityId = newProfile.Id
        };

        _context.CurrentAliases.Add(currentAlias);
        _context.Entities.Add(newProfile);
        await _context.SaveChangesAsync();
        return ControllerEnums.ProfileReturnState.Created;
    }
}
