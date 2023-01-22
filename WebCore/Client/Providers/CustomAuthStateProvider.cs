using System.Security.Claims;
using Blazored.SessionStorage;
using GlobalInfraction.WebCore.Client.Extensions;
using GlobalInfraction.WebCore.Shared.DTOs.WebEntity;
using Microsoft.AspNetCore.Components.Authorization;

namespace GlobalInfraction.WebCore.Client.Providers;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ISessionStorageService _sessionStorage;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public CustomAuthStateProvider(ISessionStorageService sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userSession = await _sessionStorage.ReadEncryptedItemAsync<UserDto>("UserSession");
            if (userSession is null) return await Task.FromResult(new AuthenticationState(_anonymous));
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new(ClaimTypes.Name, userSession.UserName),
                new(ClaimTypes.Role, userSession.Role),
            }, "JwtAuth"));

            return await Task.FromResult(new AuthenticationState(claimsPrincipal));
        }
        catch
        {
            return await Task.FromResult(new AuthenticationState(_anonymous));
        }
    }

    public async Task UpdateAuthenticationState(UserDto? userDto)
    {
        ClaimsPrincipal claimsPrincipal;
        if (userDto is not null)
        {
            claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new(ClaimTypes.Name, userDto.UserName),
                new(ClaimTypes.Role, userDto.Role),
            }));
            userDto.ExpiryTime = DateTimeOffset.UtcNow.AddMinutes(userDto.ExpiresIn);
            await _sessionStorage.SaveItemEncryptedAsync("UserSession", userDto);
        }
        else
        {
            claimsPrincipal = _anonymous;
            await _sessionStorage.RemoveItemAsync("UserSession");
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }

    public async Task<string> GetToken()
    {
        var result = string.Empty;
        try
        {
            var userSession = await _sessionStorage.ReadEncryptedItemAsync<UserDto>("UserSession");
            if (userSession is not null && DateTimeOffset.UtcNow < userSession.ExpiryTime) return userSession.Token;
        }
        catch
        {
            // ignored
        }

        return result;
    }
}
