using BanHub.Application.DTOs.WebView.CommunityProfileView;
using BanHub.WebCore.Client.Services.RestEase.Pages;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace BanHub.WebCore.Client.Pages;

partial class CommunityProfile(CommunityService communityService, NotificationService notificationService)
{
    [Parameter] public string? Identity { get; set; }

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

        _community = await communityService.GetCommunityAsync(Identity);
        _title = _community.CommunityName;
        _loading = false;
        StateHasChanged();
    }

    private async Task AuthoriseCommunityAsync()
    {
        if (_community is null || string.IsNullOrEmpty(Identity)) return;

        var state = _community.Active ? "deactivate" : "activate";
        var result = await communityService.ToggleCommunityActivationAsync(Identity);
        if (!result)
        {
            notificationService.Notify(NotificationSeverity.Error, $"Failed to {state} community!");
            return;
        }

        await LoadCommunityAsync();
        notificationService.Notify(NotificationSeverity.Success, $"Success! Community has been {state}d!");
    }
}
