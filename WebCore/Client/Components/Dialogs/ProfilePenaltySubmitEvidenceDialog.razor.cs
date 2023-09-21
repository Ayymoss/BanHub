using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHubData.Extension;
using BanHubData.Mediatr.Commands.Requests.Penalty;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace BanHub.WebCore.Client.Components.Dialogs;

partial class ProfilePenaltySubmitEvidenceDialog(NotificationService notificationService, DialogService dialogService,
    PenaltyService penaltyService)
{
    [Parameter] public required Penalty Penalty { get; set; }

    private string _evidence = string.Empty;
    private bool _processing;

    private async Task SubmitEvidence()
    {
        _processing = true;

        var videoId = _evidence.GetYouTubeVideoId();
        if (string.IsNullOrEmpty(videoId))
        {
            notificationService.Notify(NotificationSeverity.Warning, "The URL provided doesn't match. Expecting YouTube.");
            _processing = false;
            return;
        }

        var evidenceCommand = new AddPlayerPenaltyEvidenceCommand
        {
            PenaltyGuid = Penalty.PenaltyGuid,
            Evidence = videoId
        };

        var request = await penaltyService.AddPlayerPenaltyEvidenceAsync(evidenceCommand);
        if (!request)
        {
            notificationService.Notify(NotificationSeverity.Error, "Failed to add evidence!");
            _processing = false;
            dialogService.Close();
            return;
        }

        notificationService.Notify(NotificationSeverity.Success, "Evidence added!");
        _processing = false;
        dialogService.Close(evidenceCommand);
    }
}
