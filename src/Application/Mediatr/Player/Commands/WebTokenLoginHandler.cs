using System.Security.Claims;
using BanHub.Domain.Enums;
using BanHub.Domain.Interfaces.Repositories;
using BanHub.Domain.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BanHub.Application.Mediatr.Player.Commands;

public class WebTokenLoginHandler(ISignedInUsersManager signedInUsersManager, IAuthTokenRepository authTokenRepository,
        IPlayerRepository playerRepository)
    : IRequestHandler<WebTokenLoginCommand, WebTokenLoginCommandResponse>
{
    // TODO: After service restart we need to rebuild their login from the database.
    public async Task<WebTokenLoginCommandResponse> Handle(WebTokenLoginCommand request, CancellationToken cancellationToken)
    {
        if (await authTokenRepository.GetActiveTokenByTokenAsync(request.Token, cancellationToken) is not { } token)
            return CreateResponse(null);

        if (await playerRepository.GetWebUserAsync(token.PlayerId, cancellationToken) is not { } user)
            return CreateResponse(null);

        user.SignedInGuid = Guid.NewGuid().ToString();
        await authTokenRepository.ExpireAuthTokenAsync(token, cancellationToken);

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
        return CreateResponse(claimsIdentity);
    }

    private static WebTokenLoginCommandResponse CreateResponse(ClaimsIdentity? claimsIdentity)
    {
        return new WebTokenLoginCommandResponse
        {
            ReturnState = claimsIdentity is not null
                ? ControllerEnums.ReturnState.Ok
                : ControllerEnums.ReturnState.Unauthorized,
            ClaimsIdentity = claimsIdentity
        };
    }
}
