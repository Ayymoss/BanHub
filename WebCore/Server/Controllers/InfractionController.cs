using GlobalInfraction.WebCore.Server.Context;
using GlobalInfraction.WebCore.Server.Models;
using GlobalInfraction.WebCore.Shared.Enums;
using GlobalInfraction.WebCore.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlobalInfraction.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InfractionController : Controller
{
    private readonly SqliteDataContext _context;

    public InfractionController(SqliteDataContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<string>> AddInfraction([FromBody] InfractionDto request)
    {
        var user = await _context.Profiles
            .AsTracking()
            .Include(context => context.Infractions)
            .FirstOrDefaultAsync(user => user.ProfileIdentity == request.Target.ProfileIdentity);

        var admin = await _context.Profiles.FirstOrDefaultAsync(profile => profile.ProfileIdentity == request.Admin.ProfileIdentity);
        if (user is null || admin is null) return StatusCode(404, "User not found");

        // Check if the user has an existing ban and the incoming is an unban
        var infraction = user.Infractions
            .FirstOrDefault(infraction =>
                infraction.InfractionType == InfractionType.Ban
                && request.InfractionType == InfractionType.Unban
                && infraction.Instance.InstanceGuid == request.Instance.InstanceGuid);

        if (infraction is not null)
        {
            infraction.InfractionStatus = InfractionStatus.Revoked;
        }
        
        // TODO: Recursive ban. Find a way to prevent global bans triggering loads of kick infractions.

        var instance = await _context.Instances.FirstOrDefaultAsync(x => x.ApiKey == request.Instance.ApiKey);
        if (instance is null) return StatusCode(404, "Server not found");
        if (!instance.Active) return StatusCode(401, "Server is not active");

        var infractionModel = new EFInfraction
        {
            InfractionType = request.InfractionType,
            InfractionStatus = InfractionStatus.Active,
            InfractionScope = request.InfractionScope,
            InfractionGuid = Guid.NewGuid(),
            Submitted = DateTimeOffset.UtcNow,
            AdminId = admin.Id,
            Reason = request.Reason,
            Evidence = request.Evidence,
            InstanceId = instance.Id,
            TargetId = user.Id
        };

        _context.Add(infractionModel);
        await _context.SaveChangesAsync();
        return Ok($"Infraction added {infractionModel.InfractionGuid}");
    }
}
