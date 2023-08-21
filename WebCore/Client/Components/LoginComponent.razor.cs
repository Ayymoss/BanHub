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

partial class LoginComponent
{
    [Inject] protected AuthenticationStateProvider AuthStateProvider { get; set; }
    [Inject] protected NavigationManager NavigationManager { get; set; }
    [Inject] protected AuthService AuthService { get; set; }
    [Inject] protected ILocalStorageService LocalStorageService { get; set; }
    [Inject] protected DialogService DialogService { get; set; }

    private Popup _popup;
    private RadzenButton _popupButton;
    private bool _isOpen;
    private void ToggleOpen() => _isOpen = !_isOpen;

    private async Task Logout()
    {
        var response = await AuthService.LogoutAsync();
        if (response)
        {
            (AuthStateProvider as CustomAuthStateProvider)?.ClearAuthInfo();
            await LocalStorageService.RemoveItemAsync("IsAuthenticated");
            NavigationManager.NavigateTo(NavigationManager.Uri, true);
        }
    }

    private async Task OpenDialog()
    {
        await DialogService.OpenAsync<LoginDialog>("Login", options: new DialogOptions
        {
            ShowTitle = false,
            ShowClose = false,
            Style = "min-height:auto;min-width:auto;width:auto",
            CloseDialogOnOverlayClick = true
        });
    }
}
