﻿using System.Security.Claims;
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

partial class PlayerProfile : IAsyncDisposable
{
    [Parameter] public string? Identity { get; set; }

    [Inject] protected DialogService DialogService { get; set; }
    [Inject] protected AuthenticationStateProvider AuthStateProvider { get; set; }
    [Inject] protected NavigationManager NavigationManager { get; set; }
    [Inject] protected PlayerProfileService PlayerProfileService { get; set; }

    private Player? _player;
    private bool _privileged;

    private string _title = "Not Found";
    private bool _loading = true;
    private string _guid = string.Empty;
    private string _gameName = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        NavigationManager.LocationChanged += OnLocationChanged;

        var user = (await (AuthStateProvider as CustomAuthStateProvider)!.GetAuthenticationStateAsync()).User;
        var webPrivileged = user.IsInEqualOrHigherRole(WebRole.Admin);
        var communityPrivileged = user.IsInEqualOrHigherRole(CommunityRole.Moderator);
        _privileged = webPrivileged || communityPrivileged;
        Identity ??= user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        await LoadProfile();
    }

    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        var relativeUri = NavigationManager.ToBaseRelativePath(new Uri(e.Location).AbsoluteUri);

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
        _player = await PlayerProfileService.GetProfileAsync(Identity!);

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

        var options = new Radzen.DialogOptions
        {
            Style = "min-height:auto;min-width:auto;width:auto",
            CloseDialogOnOverlayClick = true
        };

        var dialog = await DialogService.OpenAsync<ProfileAddNoteDialog>("Add Note?", parameters, options);
        if (dialog is Note note)
        {
            // TODO: Handle this in a better way.
            NavigationManager.NavigateTo(NavigationManager.Uri, true);
        }
    }

    public ValueTask DisposeAsync()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
        return ValueTask.CompletedTask;
    }
}
