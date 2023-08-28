using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Mediatr.Commands.Chat;
using BanHub.WebCore.Shared.Mediatr.Commands.PlayerProfile;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Commands.Chat;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IMediator mediator, ILogger<ChatController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("AddMessages"), PluginAuthentication]
    public async Task<IActionResult> AddCommunityChatMessagesAsync([FromQuery] string authToken,
        [FromBody] AddCommunityChatMessagesCommand request)
    {
        await _mediator.Send(request);
        return Ok();
    }

    [HttpPost("Profile/Chat")]
    public async Task<ActionResult<PaginationContext<Chat>>> GetChatPaginationAsync([FromBody] GetProfileChatPaginationCommand penaltiesPagination)
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
    
    [HttpPost("Context")]
    public async Task<ActionResult<ChatContextRoot>> GetChatContextAsync([FromBody] GetMessageContextCommand chatMessageContext)
    {
        var result = await _mediator.Send(chatMessageContext);
        return Ok(result);
    }
}
