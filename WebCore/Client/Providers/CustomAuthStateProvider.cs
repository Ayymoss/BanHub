using System.Security.Claims;
using BanHub.WebCore.Shared.DTOs.WebEntity;
using Microsoft.AspNetCore.Components.Authorization;

namespace BanHub.WebCore.Client.Providers;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal _claimsPrincipal = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return new AuthenticationState(_claimsPrincipal);
    }

    public void SetAuthInfo(UserDto userProfile)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, userProfile.WebRole),
            new Claim(ClaimTypes.Role, userProfile.InstanceRole),
            new Claim(ClaimTypes.Name, userProfile.UserName)
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
