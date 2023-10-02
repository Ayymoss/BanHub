using BanHub.Application.DTOs.WebView.PlayerProfileView;
using BanHub.Application.Mediatr.Penalty.Commands;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace BanHub.WebCore.Client.Components.Dialogs;

partial class ProfilePenaltyDialog(DialogService dialogService)
{
    [Parameter] public required Penalty Penalty { get; set; }

    private async Task OpenDeleteConfirmDialog()
    {
        var parameters = new Dictionary<string, object> {{"Penalty", Penalty}};

        var options = new DialogOptions
        {
            Style = "min-height:auto;min-width:auto;width:auto",
            CloseDialogOnOverlayClick = true
        };

        var dialog = await dialogService.OpenAsync<ProfilePenaltyModifyDialog>("Modify Penalty?", parameters, options);
        if (dialog is Penalty result) dialogService.Close(result);
    }

    private async Task OpenSubmitDialog()
    {
        var parameters = new Dictionary<string, object> {{"Penalty", Penalty}};

        var options = new DialogOptions
        {
            Style = "min-height:auto;min-width:auto;width:600px",
            CloseDialogOnOverlayClick = true
        };

        var dialog = await dialogService.OpenAsync<ProfilePenaltySubmitEvidenceDialog>("Submit Evidence?", parameters, options);
        if (dialog is AddPlayerPenaltyEvidenceCommand result) dialogService.Close(result);
    }
}
