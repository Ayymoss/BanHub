using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Services;
using BanHubData.Commands.Heartbeat;
using BanHubData.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HeartbeatController : ControllerBase
{
    private readonly IMediator _mediator;

    public HeartbeatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("Community")]
    public async Task<IActionResult> CommunityHeartbeatAsync([FromBody] CommunityHeartbeatCommand request)
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
    public async Task<IActionResult> PlayersHeartbeatAsync([FromQuery] string authToken, [FromBody] PlayersHeartbeatCommand request)
    {
        await _mediator.Send(request);
        return Ok();
    }
}
