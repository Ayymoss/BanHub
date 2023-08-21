using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Shared.Commands.Penalty;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace BanHub.WebCore.Client.Components.Dialogs;

partial class ProfilePenaltyDeleteConfirmDialog
{
    [Parameter] public required Penalty Penalty { get; set; }

    [Inject] protected NotificationService NotificationService { get; set; }
    [Inject] protected DialogService DialogService { get; set; }
    [Inject] protected PenaltyService PenaltyService { get; set; }

    private string _deletionReason = string.Empty;
    private bool _processing;

    private async Task DeletePenalty()
    {
        _processing = true;

        if (_deletionReason.Length < 3)
        {
            NotificationService.Notify(NotificationSeverity.Warning, "A reason longer than 3 chars required!");
            _processing = false;
            return;
        }

        var request = await PenaltyService.DeletePenaltyAsync(new RemovePenaltyCommand
        {
            DeletionReason = _deletionReason,
            PenaltyGuid = Penalty.PenaltyGuid
        });

        if (!request)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Failed to delete penalty!");
            _processing = false;
            DialogService.Close();
            return;
        }

        NotificationService.Notify(NotificationSeverity.Success, "Penalty deleted!");
        _processing = false;
        DialogService.Close(Penalty);
    }
}
