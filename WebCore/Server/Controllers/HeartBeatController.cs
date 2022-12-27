using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Server.Interfaces;
using GlobalInfraction.WebCore.Server.Services;
using GlobalInfraction.WebCore.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace GlobalInfraction.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HeartBeatController : Controller
{
    private readonly IHeartBeatService _heartBeatService;

    public HeartBeatController(IHeartBeatService heartBeatService)
    {
        _heartBeatService = heartBeatService;
    }

    [HttpPost("Instance")]
    public async Task<ActionResult> InstanceHeartbeat([FromBody] InstanceDto request)
    {
        var result = await _heartBeatService.InstanceHeartbeat(request);
        if (result.Item1 is ControllerEnums.ProfileReturnState.NotFound) return NotFound();
        return Ok();
    }

    [HttpPost("Entities"), PluginAuthentication]
    public async Task<ActionResult> EntitiesHeartbeat([FromQuery] string authToken, [FromBody] List<EntityDto> request)
    {
        await _heartBeatService.EntitiesHeartbeat(request);
        return Ok();
    }
}
