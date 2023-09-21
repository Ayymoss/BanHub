using System.Security.Claims;
using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests;
using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Web;

public class WebTokenLoginHandler(DataContext context, ISignedInUsersManager signedInUsersManager) 
    : IRequestHandler<WebTokenLoginCommand, WebTokenLoginCommandResponse>
{
    // TODO: After service restart we need to rebuild their login from the database.
    public async Task<WebTokenLoginCommandResponse> Handle(WebTokenLoginCommand request, CancellationToken cancellationToken)
    {
        var token = await context.AuthTokens
            .AsTracking()
            .Where(x => x.Token == request.Token)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (token is null || token.Expiration < DateTimeOffset.UtcNow || token.Used)
            return new WebTokenLoginCommandResponse
            {
                ReturnState = ControllerEnums.ReturnState.Unauthorized,
                ClaimsIdentity = null
            };

        var user = await context.Players
            .Where(x => x.Id == token.PlayerId)
            .Select(x => new WebUser
            {
                UserName = x.CurrentAlias.Alias.UserName,
                WebRole = x.WebRole.ToString(),
                CommunityRole = x.CommunityRole.ToString(),
                Identity = x.Identity,
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);


        if (user is null)
            return new WebTokenLoginCommandResponse
            {
                ReturnState = ControllerEnums.ReturnState.Unauthorized,
                ClaimsIdentity = null
            };

        user.SignedInGuid = Guid.NewGuid().ToString();
        token.Used = true;
        context.AuthTokens.Update(token);
        await context.SaveChangesAsync(cancellationToken);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, $"Web_{user.WebRole}"),
            new(ClaimTypes.Role, $"Community_{user.CommunityRole}"),
            new(ClaimTypes.NameIdentifier, user.Identity),
            new("SignedInGuid", user.SignedInGuid),
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        signedInUsersManager.AddUser(user);
        return new WebTokenLoginCommandResponse
        {
            ReturnState = ControllerEnums.ReturnState.Ok,
            ClaimsIdentity = claimsIdentity
        };
    }
}
