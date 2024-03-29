﻿using BanHub.Application.Mediatr.Player.Commands;
using BanHub.Domain.ValueObjects.Services;
using BanHub.WebCore.Client.Interfaces.RestEase;
using BanHub.WebCore.Client.Utilities;
using RestEase;

namespace BanHub.WebCore.Client.Services.RestEase;

public class AuthService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif
    private readonly IAuthService _api = RestClient.For<IAuthService>(ApiHost);

    public async Task<bool> LoginAsync(WebTokenLoginCommand webLogin)
    {
        try
        {
            var response = await _api.LoginAsync(webLogin);
            return response.IsSuccessStatusCode;
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to login: {e.Message}");
        }

        return false;
    }

    public async Task<(bool, WebUser?)> UserProfileAsync()
    {
        try
        {
            var response = await _api.GetProfileAsync();
            var result = await response.DeserializeHttpResponseContentAsync<WebUser>();
            return response.IsSuccessStatusCode && result is not null ? (true, result) : (false, null);
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get profile: {e.Message}");
        }

        return (false, null);
    }

    public async Task<bool> LogoutAsync()
    {
        try
        {
            var response = await _api.LogoutAsync();
            return response.IsSuccessStatusCode;
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to logout: {e.Message}");
        }

        return false;
    }
}
