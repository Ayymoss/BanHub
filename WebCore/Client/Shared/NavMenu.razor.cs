using System.Security.Claims;
using BanHub.WebCore.Client.Providers;
using BanHub.WebCore.Client.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;

namespace BanHub.WebCore.Client.Shared;

partial class NavMenu
{
    [Inject] protected NavigationManager NavigationManager { get; set; }
    [Inject] protected AuthenticationStateProvider AuthStateProvider { get; set; }
    [Inject] protected NotificationService NotificationService { get; set; }

    private string? _identity;
    private string? _searchValue;
    private string _versionNumber = HelperMethods.GetVersionAsString();

    private void OnSearch(string arg)
    {
        if (string.IsNullOrWhiteSpace(_searchValue) || _searchValue.Length < 3)
        {
            NotificationService.Notify(NotificationSeverity.Info, "Search must be at least 3 characters long.");
            return;
        }

        NavigationManager.NavigateTo($"/Search?query={_searchValue}");
    }

    protected override async Task OnInitializedAsync()
    {
        var user = (await (AuthStateProvider as CustomAuthStateProvider)!.GetAuthenticationStateAsync()).User;
        _identity ??= user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    }
}
