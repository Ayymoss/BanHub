using GlobalBan.WebCore.Server.Context;
using GlobalBan.WebCore.Server.Models;
using GlobalBan.WebCore.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlobalBan.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstanceController : Controller
{
    private readonly SqliteDataContext _context;

    public InstanceController(SqliteDataContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<string>> CreateOrUpdate([FromBody] InstanceRequestModel request)
    {
        var user = await _context.Instances.FirstOrDefaultAsync(server => server.InstanceGuid == request.InstanceGuid);

        if (user is not null)
            return user.Active
                ? Accepted("Instance exists, and is active.")
                : Ok("Instance exists, but is not active.");

        _context.Instances.Add(new EFInstance
        {
            InstanceGuid = request.InstanceGuid,
            InstanceIp = request.InstanceIp,
            InstanceName = request.InstanceName,
            ApiKey = request.ApiKey,
            Active = false
        });

        await _context.SaveChangesAsync();

        return StatusCode(200, $"Instance added {request.InstanceGuid}");
    }

    [HttpGet("{guid}")]
    public async Task<ActionResult<InstanceRequestModel>> GetServer(string guid)
    {
        var guidParse = Guid.TryParse(guid, out var guidResult);
        if (!guidParse) return BadRequest("Invalid guid");

        var result = await _context.Instances.FirstOrDefaultAsync(x => x.InstanceGuid == guidResult);
        if (result is null) return NotFound("Instance not found");

        return Ok(new InstanceRequestModel
        {
            InstanceGuid = result.InstanceGuid,
            InstanceIp = result.InstanceIp,
            InstanceName = result.InstanceName,
            Active = result.Active
        });
    }
}
