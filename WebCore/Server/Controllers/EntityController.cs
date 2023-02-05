using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Server.Utilities;
using BanHub.WebCore.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/v2/[controller]")]
public class EntityController : ControllerBase
{
    private readonly IEntityService _entityService;

    public EntityController(IEntityService entityService)
    {
        _entityService = entityService;
    }

    // [HttpGet("Notes"),  Authorize(Roles = "InstanceModerator, InstanceAdministrator, InstanceSeniorAdmin, InstanceOwner, WebAdmin, WebSuperAdmin")]
    [HttpPost, PluginAuthentication]
    public async Task<ActionResult> CreateOrUpdate([FromQuery] string authToken, [FromBody] EntityDto request)
    {
        return await _entityService.CreateOrUpdate(request) switch
        {
            ControllerEnums.ProfileReturnState.Updated => NoContent(),
            ControllerEnums.ProfileReturnState.Created => StatusCode(StatusCodes.Status201Created),
            _ => BadRequest() // Should be unreachable
        };
    }

    [HttpPost("All")]
    public async Task<ActionResult<IEnumerable<EntityDto>>> GetEntities([FromBody] PaginationDto pagination)
    {
        return Ok(await _entityService.Pagination(pagination));
    }

    [HttpGet]
    public async Task<ActionResult<EntityDto>> GetEntity([FromQuery] string identity)
    {
        var privileged = User.IsInAnyRole("InstanceModerator", "InstanceAdministrator", "InstanceSeniorAdmin", "InstanceOwner", "WebAdmin", "WebSuperAdmin");
        var result = await _entityService.GetUser(identity, privileged);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("Exists")]
    public async Task<ActionResult<bool>> EntityExists([FromQuery] string identity)
    {
        var result = await _entityService.HasEntity(identity);
        if (!result) return NotFound(result);
        return Ok(result);
    }

    [HttpPost("GetToken"), PluginAuthentication]
    public async Task<ActionResult<string>> GetAuthenticationToken([FromQuery] string authToken, [FromBody] EntityDto request)
    {
        var result = await _entityService.GetAuthenticationToken(request);
        return Ok(result);
    }
    
}
