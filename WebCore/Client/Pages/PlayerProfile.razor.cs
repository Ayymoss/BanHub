using System.Security.Claims;
using BanHub.WebCore.Client.Components.Dialogs;
using BanHub.WebCore.Client.Providers;
using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Client.Utilities;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHubData.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Radzen;

namespace BanHub.WebCore.Client.Pages;

partial class PlayerProfile(DialogService dialogService, AuthenticationStateProvider authStateProvider, NavigationManager navigationManager,
    PlayerProfileService playerProfileService) : IAsyncDisposable
{
    [Parameter] public string? Identity { get; set; }

    private Player? _player;
    private bool _privileged;

    private string _title = "Not Found";
    private bool _loading = true;
    private string _guid = string.Empty;
    private string _gameName = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        navigationManager.LocationChanged += OnLocationChanged;

        var user = (await (authStateProvider as CustomAuthStateProvider)!.GetAuthenticationStateAsync()).User;
        var webPrivileged = user.IsInEqualOrHigherRole(WebRole.Admin);
        var communityPrivileged = user.IsInEqualOrHigherRole(CommunityRole.Moderator);
        _privileged = webPrivileged || communityPrivileged;
        Identity ??= user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        await LoadProfile();
    }

    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        var relativeUri = navigationManager.ToBaseRelativePath(new Uri(e.Location).AbsoluteUri);

        var uriParts = relativeUri.Split('/');
        if (uriParts.Length < 2 || uriParts[0] != "Players") return;

        Identity = uriParts[1];
        if (string.IsNullOrEmpty(Identity)) return;

        _loading = true;
        await LoadProfile();
        StateHasChanged();
    }

    private async Task LoadProfile()
    {
        if (string.IsNullOrEmpty(Identity)) return;
        _player = await playerProfileService.GetProfileAsync(Identity!);

        var nameSplit = _player.Identity.ToUpper().Split(':');
        _guid = nameSplit[0];
        _gameName = nameSplit[1];
        _title = _player.UserName;
        _loading = false;
    }

    private async Task AddNote()
    {
        if (_player?.Identity is null) return;
        var parameters = new Dictionary<string, object>
        {
            {"Identity", _player.Identity}
        };

        var options = new DialogOptions
        {
            Style = "min-height:auto;min-width:auto;width:auto",
            CloseDialogOnOverlayClick = true
        };

        var dialog = await dialogService.OpenAsync<ProfileAddNoteDialog>("Add Note?", parameters, options);
        if (dialog is Note note)
        {
            // TODO: Handle this in a better way.
            navigationManager.NavigateTo(navigationManager.Uri, true);
        }
    }

    private static BadgeStyle GetSentimentColour(float? sentiment)
    {
        return sentiment switch
        {
            > 0.5f => BadgeStyle.Danger,
            > 0.35f => BadgeStyle.Warning,
            > 0.1f => BadgeStyle.Info,
            > 0f => BadgeStyle.Success,
            _ => BadgeStyle.Primary
        };
    }

    private static string GetSentimentText(float? sentiment)
    {
        return sentiment switch
        {
            > 0.5f => "Very Negative",
            > 0.35f => "Negative",
            > 0.1f => "Neutral",
            > 0f => "Positive",
            _ => "--"
        };
    }

    public ValueTask DisposeAsync()
    {
        navigationManager.LocationChanged -= OnLocationChanged;
        return ValueTask.CompletedTask;
    }
}
