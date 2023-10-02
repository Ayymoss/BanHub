using System.Security.Claims;
using BanHub.Domain.Enums;
using BanHub.Domain.ValueObjects.Services;
using BanHub.WebCore.Client.Utilities;
using Microsoft.AspNetCore.Components.Authorization;

namespace BanHub.WebCore.Client.Providers;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal _claimsPrincipal = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return new AuthenticationState(_claimsPrincipal);
    }

    public void SetAuthInfo(WebUser webUserProfile)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, HelperMethods.GetRolesAsString(webRoles: new [] {Enum.Parse<WebRole>(webUserProfile.WebRole)})),
            new Claim(ClaimTypes.Role, HelperMethods.GetRolesAsString(communityRoles: new [] {Enum.Parse<CommunityRole>(webUserProfile.CommunityRole)})),
            new Claim(ClaimTypes.Name, webUserProfile.UserName),
            new Claim(ClaimTypes.NameIdentifier, webUserProfile.Identity),
            new("SignedInGuid", webUserProfile.SignedInGuid)
        }, "AuthCookie");

        _claimsPrincipal = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void ClearAuthInfo()
    {
        _claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
