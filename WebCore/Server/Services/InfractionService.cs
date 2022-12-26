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
    private readonly SqliteDataContext _context;

    public InfractionService(SqliteDataContext context)
    {
        _context = context;
    }

    public async Task<(ControllerEnums.ProfileReturnState, Guid?)> AddInfraction([FromBody] InfractionDto request)
    {
        var user = await _context.Entities
            .AsTracking()
            .FirstOrDefaultAsync(user => user.Identity == request.Target!.Identity);

        var admin = await _context.Entities
            .FirstOrDefaultAsync(profile => profile.Identity == request.Admin!.Identity);

        if (user is null || admin is null) return (ControllerEnums.ProfileReturnState.NotFound, null);

        // Check if the user has an existing ban and the incoming is an unban
        var infraction = await _context.Infractions
            .AsTracking()
            .Where(inf => inf.TargetId == user.Id && inf.Instance.InstanceGuid == request.Instance!.InstanceGuid)
            .FirstOrDefaultAsync(inf => inf.InfractionType == InfractionType.Ban
                                        && inf.InfractionStatus == InfractionStatus.Active);

        if (infraction is not null)
        {
            switch (infraction.InfractionType)
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


        // TODO: Recursive ban. Find a way to prevent global bans triggering loads of kick infractions.

        var instance = await _context.Instances.FirstOrDefaultAsync(x => x.ApiKey == request.Instance!.ApiKey);
        if (instance is null) return (ControllerEnums.ProfileReturnState.NotFound, null);

        var infractionModel = new EFInfraction
        {
            InfractionType = request.InfractionType!.Value,
            InfractionStatus = InfractionStatus.Active,
            InfractionScope = request.InfractionScope!.Value,
            InfractionGuid = Guid.NewGuid(),
            Submitted = DateTimeOffset.UtcNow,
            AdminId = admin.Id,
            Reason = request.Reason!,
            Evidence = request.Evidence,
            InstanceId = instance.Id,
            TargetId = user.Id
        };

        _context.Add(infractionModel);
        await _context.SaveChangesAsync();
        return (ControllerEnums.ProfileReturnState.Created, infractionModel.InfractionGuid);
    }
}
