using System.Security.Claims;
using BanHub.WebCore.Server.Interfaces;
using Data.Domains.WebEntity;
using Data.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("Login")]
    public async Task<IActionResult> LoginAsync([FromBody] WebLoginRequest webLoginRequest)
    {
        var result = await _authService.LoginAsync(webLoginRequest);
        switch (result.Item1)
        {
            case ControllerEnums.ReturnState.Ok:
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(result.Item2), result.Item3);
                return Ok("Success");
        }
        return Unauthorized("Token or User is invalid.");
    }

    [HttpGet("Profile"), Authorize]
    public async Task<ActionResult<WebUser>> UserProfileAsync()
    {
        var userId = HttpContext.User.Claims
            .Where(x => x.Type == "UserId")
            .Select(f => Convert.ToInt32(f.Value))
            .First();

        var result = await _authService.UserProfileAsync(userId);
        if (result is null) return BadRequest("User is invalid.");
        return Ok(result);
    }

    [HttpPost("Logout"), Authorize]
    public async Task<IActionResult> LogoutAsync()
    {
        await HttpContext.SignOutAsync();
        return Ok("Success");
    }
}
