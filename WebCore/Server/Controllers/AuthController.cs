using System.Security.Claims;
using BanHub.WebCore.Shared.Commands.Web;
using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("Login")]
    public async Task<IActionResult> LoginAsync([FromBody] WebTokenLoginCommand webLoginRequest)
    {
        var result = await _mediator.Send(webLoginRequest);

        if (result.ReturnState is not ControllerEnums.ReturnState.Ok) return Unauthorized("Token or User is invalid.");

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(result.ClaimsIdentity!), new AuthenticationProperties());
        return Ok("Success");
    }

    [HttpGet("Profile"), Authorize]
    public async Task<ActionResult<WebUser>> UserProfileAsync()
    {
        var signedInGuid = User.Claims.FirstOrDefault(c => c.Type == "SignedInGuid")?.Value;
        if (signedInGuid is null) return Unauthorized("You are not authorised to perform this action");
        var result = await _mediator.Send(new GetUserProfileCommand {SignedInGuid = signedInGuid});
        if (result is null) return Unauthorized("You are not authorised to perform this action");
        return Ok(result);
    }

    [HttpPost("Logout"), Authorize]
    public async Task<IActionResult> LogoutAsync()
    {
        await HttpContext.SignOutAsync();
        return Ok("Success");
    }
}
