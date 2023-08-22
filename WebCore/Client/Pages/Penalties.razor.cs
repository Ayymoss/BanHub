using BanHub.WebCore.Client.Components.Dialogs;
using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Shared.Commands.Penalty;
using BanHub.WebCore.Shared.Models.PenaltiesView;
using BanHubData.Commands.Penalty;
using Humanizer;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace BanHub.WebCore.Client.Pages;

partial class Penalties
{
    [Inject] protected PenaltyService PenaltyService { get; set; }
    [Inject] protected DialogService DialogService { get; set; }

    private RadzenDataGrid<Penalty> _dataGrid;
    private IEnumerable<Penalty> _playerTable;
    private bool _isLoading = true;
    private int _count;
    private string? _searchString;

    private async Task LoadData(LoadDataArgs args)
    {
        _isLoading = true;
        var paginationQuery = new GetPenaltiesPaginationCommand
        {
            Sorts = args.Sorts,
            SearchString = _searchString,
            Top = args.Top ?? 20,
            Skip = args.Skip ?? 0
        };

        var context = await PenaltyService.GetPenaltiesPaginationAsync(paginationQuery);
        _playerTable = context.Data;
        _count = context.Count;
        _isLoading = false;
    }

    private void OnSearch(string text)
    {
        _searchString = text;
        _dataGrid.Reload();
    }

    private async Task RowClickEvent(DataGridRowMouseEventArgs<Penalty> arg)
    {
        var convertedPenalty = new BanHub.WebCore.Shared.Models.PlayerProfileView.Penalty
        {
            PenaltyGuid = arg.Data.PenaltyGuid,
            IssuerUserName = arg.Data.IssuerUserName,
            IssuerIdentity = arg.Data.IssuerIdentity,
            Reason = arg.Data.Reason,
            Evidence = arg.Data.Evidence,
            CommunityName = arg.Data.CommunityName,
            CommunityGuid = arg.Data.CommunityGuid,
            Expiration = arg.Data.Expiration,
            PenaltyType = arg.Data.PenaltyType,
            PenaltyScope = arg.Data.PenaltyScope,
            PenaltyStatus = arg.Data.PenaltyStatus,
            Submitted = arg.Data.Submitted
        };

        var parameters = new Dictionary<string, object> {{"Penalty", convertedPenalty}};
        var options = new Radzen.DialogOptions
        {
            Style = "min-height:auto;min-width:970px;max-height:97%;",
            CloseDialogOnOverlayClick = true
        };

        var title = $"{arg.Data.PenaltyType} - {arg.Data.RecipientUserName} - {arg.Data.Submitted.Humanize().Titleize()}";
        var dialog = await DialogService.OpenAsync<ProfilePenaltyDialog>(title, parameters, options);

        switch (dialog)
        {
            case Penalty penalty:
                await _dataGrid.Reload();
                break;
            case AddPlayerPenaltyEvidenceCommand evidence:
                await _dataGrid.Reload();
                break;
        }
    }
}
