using GlobalInfraction.WebCore.Server.Context;
using GlobalInfraction.WebCore.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlobalInfraction.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HeartbeatController : Controller
{
    private readonly SqliteDataContext _context;

    public HeartbeatController(SqliteDataContext context)
    {
        _context = context;
    }

    [HttpPost("Instance")]
    public async Task<ActionResult<bool>> InstanceHeartbeat([FromBody] InstanceDto request)
    {
        var instance = await _context.Instances
            .AsTracking()
            .FirstOrDefaultAsync(x => x.InstanceGuid == request.InstanceGuid && x.ApiKey == request.ApiKey);
        if (instance is null) return NotFound();
        
        instance.HeartBeat = DateTimeOffset.UtcNow;
        _context.Instances.Update(instance);
        await _context.SaveChangesAsync();
        return Ok(instance.Active);
    }

    [HttpPost("Profiles")]
    public async Task<ActionResult> ProfilesHeartbeat([FromBody] List<EntityDto> request)
    {
        var profiles = await _context.Entities
            .AsTracking()
            .Where(p => request
                .Select(r => r.ProfileIdentity)
                .Contains(p.ProfileIdentity))
            .ToListAsync();

        foreach (var profile in profiles)
        {
            profile.HeartBeat = DateTimeOffset.UtcNow;
            _context.Entities.Update(profile);
        }

        await _context.SaveChangesAsync();
        return Ok();
        
      
    }
}
