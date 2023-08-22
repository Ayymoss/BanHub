﻿using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Shared.Models.CommunityProfileView;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace BanHub.WebCore.Client.Pages;

partial class CommunityProfile
{
    [Parameter] public string? Identity { get; set; }

    [Inject] protected CommunityService CommunityService { get; set; }
    [Inject] protected NotificationService NotificationService { get; set; }

    private Community? _community;
    private string _title = "Not Found";
    private bool _loading = true;

    private bool HasSocials => _community is not null && _community.Socials is not null and not {Count: 0};

    protected override async Task OnInitializedAsync()
    {
        await LoadCommunityAsync();
    }

    private async Task LoadCommunityAsync()
    {
        if (string.IsNullOrEmpty(Identity)) return;

        _community = await CommunityService.GetCommunityAsync(Identity);
        _title = _community.CommunityName;
        _loading = false;
        StateHasChanged();
    }

    private async Task AuthoriseCommunityAsync()
    {
        if (_community is null || string.IsNullOrEmpty(Identity)) return;

        var state = _community.Active ? "deactivate" : "activate";
        var result = await CommunityService.ToggleCommunityActivationAsync(Identity);
        if (!result)
        {
            NotificationService.Notify(NotificationSeverity.Error, $"Failed to {state} community!");
            return;
        }

        await LoadCommunityAsync();
        NotificationService.Notify(NotificationSeverity.Success, $"Success! Community has been {state}d!");
    }
}