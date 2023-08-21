using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHubData.Commands.Penalty;
using BanHubData.Extension;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace BanHub.WebCore.Client.Components.Dialogs;

partial class ProfilePenaltySubmitEvidenceDialog
{
    [Parameter] public required Penalty Penalty { get; set; }

    [Inject] protected NotificationService NotificationService { get; set; }
    [Inject] protected DialogService DialogService { get; set; }
    [Inject] protected PenaltyService PenaltyService { get; set; }

    private string _evidence = string.Empty;
    private bool _processing;

    private async Task SubmitEvidence()
    {
        _processing = true;

        var videoId = _evidence.GetYouTubeVideoId();
        if (string.IsNullOrEmpty(videoId))
        {
            NotificationService.Notify(NotificationSeverity.Warning, "The URL provided doesn't match. Expecting YouTube.");
            _processing = false;
            return;
        }

        var evidenceCommand = new AddPlayerPenaltyEvidenceCommand
        {
            PenaltyGuid = Penalty.PenaltyGuid,
            Evidence = videoId
        };

        var request = await PenaltyService.AddPlayerPenaltyEvidenceAsync(evidenceCommand);
        if (!request)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Failed to add evidence!");
            _processing = false;
            DialogService.Close();
            return;
        }

        NotificationService.Notify(NotificationSeverity.Success, "Evidence added!");
        _processing = false;
        DialogService.Close(evidenceCommand);
    }
}
