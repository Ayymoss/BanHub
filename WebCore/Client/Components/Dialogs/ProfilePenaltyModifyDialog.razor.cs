using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.Penalty;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHubData.Enums;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace BanHub.WebCore.Client.Components.Dialogs;

partial class ProfilePenaltyModifyDialog
{
    [Parameter] public required Penalty Penalty { get; set; }

    [Inject] protected NotificationService NotificationService { get; set; }
    [Inject] protected DialogService DialogService { get; set; }
    [Inject] protected PenaltyService PenaltyService { get; set; }

    private string _deletionReason = string.Empty;
    private bool _processing;
    private ModifyPenalty _modifyPenalty;

    private async Task ModifyPenalty()
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
            PenaltyGuid = Penalty.PenaltyGuid,
            ModifyPenalty = _modifyPenalty
        });

        if (!request)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Failed to modify penalty!");
            _processing = false;
            DialogService.Close();
            return;
        }

        NotificationService.Notify(NotificationSeverity.Success, "Penalty modified!");
        _processing = false;
        DialogService.Close(Penalty);
    }
}
