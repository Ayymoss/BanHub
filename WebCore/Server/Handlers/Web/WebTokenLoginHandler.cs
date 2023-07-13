using System.Security.Claims;
using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Commands;
using BanHub.WebCore.Shared.Commands.Web;
using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web;

public class WebTokenLoginHandler : IRequestHandler<WebTokenLoginCommand, WebTokenLoginCommandResponse>
{
    private readonly DataContext _context;

    public WebTokenLoginHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<WebTokenLoginCommandResponse> Handle(WebTokenLoginCommand request, CancellationToken cancellationToken)
    {
        var token = await _context.AuthTokens
            .AsTracking()
            .Where(x => x.Token == request.Token)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (token is null || token.Created + TimeSpan.FromMinutes(5) < DateTimeOffset.UtcNow || token.Used)
            return new WebTokenLoginCommandResponse
            {
                ReturnState = ControllerEnums.ReturnState.Unauthorized,
                ClaimsIdentity = null,
                AuthenticationProperties = null
            };

        var user = await _context.Players
            .Where(x => x.Id == token.EntityId)
            .Select(x => new WebUser
            {
                UserName = x.CurrentAlias.Alias.UserName,
                WebRole = x.WebRole.ToString(),
                InstanceRole = x.InstanceRole.ToString(),
                Identity = x.Identity
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (user is null)
            return new WebTokenLoginCommandResponse
            {
                ReturnState = ControllerEnums.ReturnState.Unauthorized,
                ClaimsIdentity = null,
                AuthenticationProperties = null
            };

        token.Used = true;
        _context.AuthTokens.Update(token);
        await _context.SaveChangesAsync(cancellationToken);

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

        return new WebTokenLoginCommandResponse
        {
            ReturnState = ControllerEnums.ReturnState.Ok,
            ClaimsIdentity = claimsIdentity,
            AuthenticationProperties = authProperties
        };
    }
}
