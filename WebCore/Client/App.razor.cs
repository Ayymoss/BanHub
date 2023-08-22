using BanHub.WebCore.Client.Providers;
using BanHub.WebCore.Client.Services.RestEase;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;

namespace BanHub.WebCore.Client;

partial class App
{
    [Inject] protected ILocalStorageService LocalStorageService { get; set; }
    [Inject] protected AuthService AuthService { get; set; }
    [Inject] protected AuthenticationStateProvider AuthStateProvider { get; set; }

    private async Task OnNavigateAsync(NavigationContext args)
    {
        var auth = await LocalStorageService.GetItemAsync<string>("IsAuthenticated");
        var user = (await (AuthStateProvider as CustomAuthStateProvider)!.GetAuthenticationStateAsync()).User;

        if (!string.IsNullOrEmpty(auth) && !user.Identity!.IsAuthenticated)
        {
            var response = await AuthService.UserProfileAsync();
            if (response.Item1)
            {
                (AuthStateProvider as CustomAuthStateProvider)?.SetAuthInfo(response.Item2!);
                return;
            }

            (AuthStateProvider as CustomAuthStateProvider)?.ClearAuthInfo();
            await LocalStorageService.RemoveItemAsync("IsAuthenticated");
        }
    }
}
