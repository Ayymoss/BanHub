using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHubData.Commands.Penalty;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace BanHub.WebCore.Client.Components.Dialogs;

partial class ProfilePenaltyDialog
{
    [Parameter] public required Penalty Penalty { get; set; }

    [Inject] protected DialogService DialogService { get; set; }

    private async Task OpenDeleteConfirmDialog()
    {
        var parameters = new Dictionary<string, object> {{"Penalty", Penalty}};

        var options = new DialogOptions
        {
            Style = "min-height:auto;min-width:auto;width:auto",
            CloseDialogOnOverlayClick = true
        };

        var dialog = await DialogService.OpenAsync<ProfilePenaltyDeleteConfirmDialog>("Delete Penalty?", parameters, options);
        if (dialog is Penalty result) DialogService.Close(result);
    }

    private async Task OpenSubmitDialog()
    {
        var parameters = new Dictionary<string, object> {{"Penalty", Penalty}};

        var options = new DialogOptions
        {
            Style = "min-height:auto;min-width:auto;width:600px",
            CloseDialogOnOverlayClick = true
        };

        var dialog = await DialogService.OpenAsync<ProfilePenaltySubmitEvidenceDialog>("Submit Evidence?", parameters, options);
        if (dialog is AddPlayerPenaltyEvidenceCommand result) DialogService.Close(result);
    }
}
