using BanHub.Application.DTOs.WebView.PlayerProfileView;
using BanHub.Application.Mediatr.Chat.Commands;
using BanHub.Application.Mediatr.Chat.Events;
using BanHub.Application.Mediatr.Player.Commands;
using BanHub.Domain.ValueObjects.Services;
using BanHub.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController(ISender sender) : ControllerBase
{

    [HttpPost("AddMessages"), PluginAuthentication]
    public async Task<IActionResult> AddCommunityChatMessagesAsync([FromBody] AddCommunityChatMessagesNotification request)
    {
        await sender.Send(request);
        return Ok();
    }

    [HttpPost("Profile/Chat")]
    public async Task<ActionResult<PaginationContext<Chat>>> GetChatPaginationAsync(
        [FromBody] GetProfileChatPaginationCommand penaltiesPagination)
    {
        var result = await sender.Send(penaltiesPagination);
        return Ok(result);
    }

    [HttpGet("Count/{identity}")]
    public async Task<ActionResult<ChatCount>> GetChatCountAsync([FromRoute] string identity)
    {
        var result = await sender.Send(new GetChatCountCommand {PlayerIdentity = identity});
        return Ok(result);
    }

    [HttpPost("Context")]
    public async Task<ActionResult<ChatContextRoot>> GetChatContextAsync([FromBody] GetMessageContextCommand chatMessageContext)
    {
        var result = await sender.Send(chatMessageContext);
        return Ok(result);
    }
}
