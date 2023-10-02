using BanHub.WebCore.Client.Providers;
using BanHub.WebCore.Client.Services.RestEase;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;

namespace BanHub.WebCore.Client;

partial class App(ILocalStorageService localStorageService, AuthService authService, AuthenticationStateProvider authStateProvider)
{
    private async Task OnNavigateAsync(NavigationContext args)
    {
        var auth = await localStorageService.GetItemAsync<string>("IsAuthenticated");
        var user = (await (authStateProvider as CustomAuthStateProvider)!.GetAuthenticationStateAsync()).User;

        if (!string.IsNullOrEmpty(auth) && !user.Identity!.IsAuthenticated)
        {
            var response = await authService.UserProfileAsync();
            if (response.Item1)
            {
                (authStateProvider as CustomAuthStateProvider)?.SetAuthInfo(response.Item2!);
                return;
            }

            (authStateProvider as CustomAuthStateProvider)?.ClearAuthInfo();
            await localStorageService.RemoveItemAsync("IsAuthenticated");
        }
    }
}
