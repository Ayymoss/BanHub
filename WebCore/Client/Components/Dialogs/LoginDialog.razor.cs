using BanHub.WebCore.Client.Services.RestEase;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace BanHub.WebCore.Client.Components.Dialogs;

public partial class LoginDialog(NavigationManager navigationManager, AuthService authService, ILocalStorageService localStorageService)
{
    private string? _token;
    private bool _processing;
    private string? _error;

    private async Task UserLogin()
    {
        _processing = true;
        _error = null;
        if (string.IsNullOrEmpty(_token))
        {
            _processing = false;
            _error = "Token is required";
            return;
        }

        if (_token.Length is not 6)
        {
            _error = "Invalid token length";
            _processing = false;
            return;
        }

        var tokenModel = new WebTokenLoginCommand {Token = _token};
        var success = await authService.LoginAsync(tokenModel);
        if (success)
        {
            await localStorageService.SetItemAsStringAsync("IsAuthenticated", "true");
            navigationManager.NavigateTo(navigationManager.Uri, true);
            return;
        }

        _error = "Invalid or expired token";
        _processing = false;
    }
}
