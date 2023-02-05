using System.Security.Claims;
using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.DTOs.WebEntity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/v2/[controller]")]
public class AuthController : ControllerBase
{
    private readonly DataContext _context;

    public AuthController(DataContext context)
    {
        _context = context;
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
    {
        var token = await _context.AuthTokens
            .AsTracking()
            .Where(x => x.Token == loginRequest.Token)
            .FirstOrDefaultAsync();

        if (token is null || token.Created + TimeSpan.FromMinutes(5) < DateTimeOffset.UtcNow || token.Used)
            return Unauthorized("Token is invalid.");

        var user = await _context.Entities
            .Where(x => x.Id == token.EntityId)
            .Select(x => new UserDto
            {
                UserName = x.CurrentAlias.Alias.UserName,
                WebRole = x.WebRole.ToString(),
                InstanceRole = x.InstanceRole.ToString()
            }).FirstOrDefaultAsync();

        if (user is null) return Unauthorized("User is invalid.");

        token.Used = true;
        _context.AuthTokens.Update(token);
        await _context.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, user.WebRole),
            new(ClaimTypes.Role, user.InstanceRole),
            new("UserId", token.EntityId.ToString())
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties();

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity), authProperties);

        return Ok("Success");
    }

    [HttpGet("Profile"), Authorize]
    public async Task<ActionResult<UserDto>> UserProfileAsync()
    {
        var userId = HttpContext.User.Claims
            .Where(x => x.Type == "UserId")
            .Select(f => Convert.ToInt32(f.Value))
            .First();

        var user = await _context.Entities
            .Where(x => x.Id == userId)
            .Select(f => new UserDto
            {
                UserName = f.CurrentAlias.Alias.UserName,
                WebRole = f.WebRole.ToString(),
                InstanceRole = f.InstanceRole.ToString()
            }).FirstOrDefaultAsync();

        if (user is null) return BadRequest("User is invalid.");

        return Ok(user);
    }

    [HttpPost("Logout"), Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return Ok("Success");
    }
    
}
