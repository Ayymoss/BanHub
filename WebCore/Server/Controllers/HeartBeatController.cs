using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Services;
using Data.Commands.Heartbeat;
using Data.Domains;
using Data.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HeartBeatController : ControllerBase
{
    private readonly IMediator _mediator;

    public HeartBeatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("Instance")]
    public async Task<IActionResult> InstanceHeartbeat([FromBody] InstanceHeartbeatCommand request)
    {
        var result = await _mediator.Send(request);
        return result switch
        {
            ControllerEnums.ReturnState.Accepted => Accepted(),
            ControllerEnums.ReturnState.Ok => Ok(),
            _ => NotFound()
        };
    }

    [HttpPost("Players"), PluginAuthentication]
    public async Task<IActionResult> EntitiesHeartbeat([FromQuery] string authToken, [FromBody] PlayersHeartbeartCommand request)
    {
        await _mediator.Send(request);
        return Ok();
    }
}
