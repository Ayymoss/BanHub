using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Commands.Chat;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHubData.Commands.Chat;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("AddMessages"), PluginAuthentication]
    public async Task<IActionResult> AddCommunityChatMessagesAsync([FromQuery] string authToken,
        [FromBody] AddCommunityChatMessagesCommand request)
    {
        await _mediator.Send(request);
        return Ok();
    }

    [HttpPost("Pagination")]
    public async Task<ActionResult<Chat>> GetChatPaginationAsync(
        [FromBody] GetChatPaginationCommand penaltiesPagination)
    {
        var result = await _mediator.Send(penaltiesPagination);
        return Ok(result);
    }

    [HttpGet("Count/{identity}")]
    public async Task<ActionResult<ChatCount>> GetChatCountAsync([FromRoute] string identity)
    {
        var result = await _mediator.Send(new GetChatCountCommand {PlayerIdentity = identity});
        return Ok(result);
    }
}
