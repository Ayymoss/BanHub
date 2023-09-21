using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.Servers;
using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Enums;
using BanHubData.Mediatr.Commands.Requests.Server;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServerController(ISender sender) : ControllerBase
{
    [HttpPost, PluginAuthentication]
    public async Task<IActionResult> CreateOrUpdateServerAsync([FromBody] CreateOrUpdateServerCommand request)
    {
        var result = await sender.Send(request);
        return result switch
        {
            ControllerEnums.ReturnState.NotFound => NotFound(),
            ControllerEnums.ReturnState.NoContent => NoContent(),
            ControllerEnums.ReturnState.Ok => Ok(),
            _ => BadRequest()
        };
    }

    [HttpPost("Pagination")]
    public async Task<ActionResult<PaginationContext<Shared.Models.ServersView.Server>>> GetServersPaginationAsync(
        [FromBody] GetServersPaginationCommand playersPagination)
    {
        var result = await sender.Send(playersPagination);
        return Ok(result);
    }
}
