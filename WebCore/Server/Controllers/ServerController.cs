using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Services;
using BanHubData.Commands.Instance.Server;
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
    public async Task<ActionResult> Add([FromQuery] string authToken, [FromBody] CreateOrUpdateServerCommand request)
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

    //[HttpGet] // TODO: Check if used
    //public async Task<ActionResult<BanHubData.Domains.Server>> Get([FromQuery] string serverId)
    //{
    //    var result = await _serverService.GetAsync(serverId);
    //    return result.Item1 switch
    //    {
    //        ControllerEnums.ReturnState.NotFound => NotFound(),
    //        ControllerEnums.ReturnState.Ok => Ok(result.Item2),
    //        _ => BadRequest()
    //    };
    //}
}
