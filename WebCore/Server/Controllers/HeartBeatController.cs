using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/v2/[controller]")]
public class HeartBeatController : ControllerBase
{
    private readonly IHeartBeatService _heartBeatService;

    public HeartBeatController(IHeartBeatService heartBeatService)
    {
        _heartBeatService = heartBeatService;
    }

    [HttpPost("Instance")]
    public async Task<ActionResult> InstanceHeartbeat([FromBody] InstanceDto request)
    {
        var result = await _heartBeatService.InstanceHeartbeatAsync(request);
        if (result.Item1 is ControllerEnums.ReturnState.NotFound) return NotFound();
        return Ok();
    }

    [HttpPost("Entities"), PluginAuthentication]
    public async Task<ActionResult> EntitiesHeartbeat([FromQuery] string authToken, [FromBody] List<EntityDto> request)
    {
        await _heartBeatService.EntitiesHeartbeatAsync(request);
        return Ok();
    }
}
