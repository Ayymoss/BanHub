using BanHub.Application.DTOs.WebView.PenaltiesView;
using BanHub.Application.Mediatr.Penalty.Commands;
using BanHub.Domain.Enums;
using BanHub.WebCore.Client.Components.Dialogs;
using BanHub.WebCore.Client.Services.RestEase.Pages;
using Humanizer;
using Radzen;
using Radzen.Blazor;
using SortDescriptor = BanHub.Domain.ValueObjects.Services.SortDescriptor;

namespace BanHub.WebCore.Client.Pages;

partial class Penalties(PenaltyService penaltyService, DialogService dialogService)
{
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
            Sorts = args.Sorts.Select(x => new SortDescriptor
            {
                Property = x.Property,
                SortOrder = x.SortOrder == SortOrder.Ascending
                    ? SortDirection.Ascending
                    : SortDirection.Descending
            }),
            SearchString = _searchString,
            Top = args.Top ?? 20,
            Skip = args.Skip ?? 0
        };

        var context = await penaltyService.GetPenaltiesPaginationAsync(paginationQuery);
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
        var convertedPenalty = new Application.DTOs.WebView.PlayerProfileView.Penalty
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
            Submitted = arg.Data.Submitted,
            Automated = arg.Data.Automated
        };

        var parameters = new Dictionary<string, object> {{"Penalty", convertedPenalty}};
        var options = new DialogOptions
        {
            Style = "width:100%; max-width:800px; min-height:auto; min-width:auto;",
            CloseDialogOnOverlayClick = true
        };

        var title = $"{arg.Data.PenaltyType} - {arg.Data.RecipientUserName} - {arg.Data.Submitted.Humanize().Titleize()}";
        var dialog = await dialogService.OpenAsync<ProfilePenaltyDialog>(title, parameters, options);

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
