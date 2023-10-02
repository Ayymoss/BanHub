using BanHub.Application.Mediatr.Server.Commands;
using BanHub.Domain.Enums;
using BanHub.Domain.ValueObjects.Services;
using BanHub.Infrastructure.Services;
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
    public async Task<ActionResult<PaginationContext<Application.DTOs.WebView.ServersView.Server>>> GetServersPaginationAsync(
        [FromBody] GetServersPaginationCommand playersPagination)
    {
        var result = await sender.Send(playersPagination);
        return Ok(result);
    }
}
