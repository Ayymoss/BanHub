using BanHub.Application.DTOs.WebView.PlayerProfileView;
using BanHub.Application.Mediatr.Penalty.Commands;
using BanHub.Domain.Enums;
using BanHub.WebCore.Client.Services.RestEase.Pages;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace BanHub.WebCore.Client.Components.Dialogs;

partial class ProfilePenaltyModifyDialog(NotificationService notificationService, DialogService dialogService,
    PenaltyService penaltyService)
{
    [Parameter] public required Penalty Penalty { get; set; }
    
    private string _deletionReason = string.Empty;
    private bool _processing;
    private ModifyPenalty _modifyPenalty;

    private async Task ModifyPenalty()
    {
        _processing = true;

        if (_deletionReason.Length < 3)
        {
            notificationService.Notify(NotificationSeverity.Warning, "A reason longer than 3 chars required!");
            _processing = false;
            return;
        }

        var request = await penaltyService.DeletePenaltyAsync(new RemovePenaltyCommand
        {
            DeletionReason = _deletionReason,
            PenaltyGuid = Penalty.PenaltyGuid,
            ModifyPenalty = _modifyPenalty
        });

        if (!request)
        {
            notificationService.Notify(NotificationSeverity.Error, "Failed to modify penalty!");
            _processing = false;
            dialogService.Close();
            return;
        }

        notificationService.Notify(NotificationSeverity.Success, "Penalty modified!");
        _processing = false;
        dialogService.Close(Penalty);
    }
}
