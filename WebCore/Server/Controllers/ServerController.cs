using BanHub.WebCore.Server.Services;
using BanHubData.Commands.Community.Server;
using BanHubData.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServerController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServerController( IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost, PluginAuthentication]
    public async Task<IActionResult> CreateOrUpdateServerAsync([FromQuery] string authToken, [FromBody] CreateOrUpdateServerCommand request)
    {
        var result = await _mediator.Send(request);
        return result switch
        {
            ControllerEnums.ReturnState.NotFound => NotFound(),
            ControllerEnums.ReturnState.NoContent => NoContent(),
            ControllerEnums.ReturnState.Ok => Ok(),
            _ => BadRequest()
        };
    }
}
