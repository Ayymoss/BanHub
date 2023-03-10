using System.Security.Claims;
using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Shared.DTOs.WebEntity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Services;

public class AuthService : IAuthService
{
    private readonly DataContext _context;

    public AuthService(DataContext context)
    {
        _context = context;
    }

    public async Task<(ControllerEnums.ReturnState, ClaimsIdentity, AuthenticationProperties)> LoginAsync(LoginRequestDto loginRequest)
    {
        var token = await _context.AuthTokens
            .AsTracking()
            .Where(x => x.Token == loginRequest.Token)
            .FirstOrDefaultAsync();

        if (token is null || token.Created + TimeSpan.FromMinutes(5) < DateTimeOffset.UtcNow || token.Used)
            return (ControllerEnums.ReturnState.Unauthorized, null, null)!;

        var user = await _context.Entities
            .Where(x => x.Id == token.EntityId)
            .Select(x => new UserDto
            {
                UserName = x.CurrentAlias.Alias.UserName,
                WebRole = x.WebRole.ToString(),
                InstanceRole = x.InstanceRole.ToString(),
                Identity = x.Identity
            }).FirstOrDefaultAsync();

        if (user is null) return (ControllerEnums.ReturnState.Unauthorized, null, null)!;

        token.Used = true;
        _context.AuthTokens.Update(token);
        await _context.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, user.WebRole),
            new(ClaimTypes.Role, user.InstanceRole),
            new(ClaimTypes.NameIdentifier, user.Identity),
            new("UserId", token.EntityId.ToString())
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties();
        
        return (ControllerEnums.ReturnState.Ok, claimsIdentity, authProperties);
    }

    public async Task<UserDto?> UserProfileAsync(int userId)
    {
        var user = await _context.Entities
            .Where(x => x.Id == userId)
            .Select(f => new UserDto
            {
                UserName = f.CurrentAlias.Alias.UserName,
                WebRole = f.WebRole.ToString(),
                InstanceRole = f.InstanceRole.ToString(),
                Identity = f.Identity
            }).FirstOrDefaultAsync();

        return user;
    }
}
