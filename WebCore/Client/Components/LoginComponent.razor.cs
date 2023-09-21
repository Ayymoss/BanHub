using BanHub.WebCore.Client.Components.Dialogs;
using BanHub.WebCore.Client.Providers;
using BanHub.WebCore.Client.Services.RestEase;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;
using Radzen.Blazor;
using Radzen.Blazor.Rendering;

namespace BanHub.WebCore.Client.Components;

partial class LoginComponent(AuthenticationStateProvider authStateProvider, NavigationManager navigationManager, AuthService authService,
    ILocalStorageService localStorageService, DialogService dialogService)
{
    private Popup _popup;
    private RadzenButton _popupButton;
    private bool _isOpen;
    private void ToggleOpen() => _isOpen = !_isOpen;

    private async Task Logout()
    {
        var response = await authService.LogoutAsync();
        if (response)
        {
            (authStateProvider as CustomAuthStateProvider)?.ClearAuthInfo();
            await localStorageService.RemoveItemAsync("IsAuthenticated");
            navigationManager.NavigateTo(navigationManager.Uri, true);
        }
    }

    private async Task OpenDialog()
    {
        await dialogService.OpenAsync<LoginDialog>("Login", options: new DialogOptions
        {
            ShowTitle = false,
            ShowClose = false,
            Style = "min-height:auto;min-width:auto;width:auto",
            CloseDialogOnOverlayClick = true
        });
    }
}
