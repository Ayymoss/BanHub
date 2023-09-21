using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.Chat;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.PlayerProfile;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Mediatr.Commands.Requests.Chat;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController(ISender sender) : ControllerBase
{

    [HttpPost("AddMessages"), PluginAuthentication]
    public async Task<IActionResult> AddCommunityChatMessagesAsync([FromBody] AddCommunityChatMessagesCommand request)
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
