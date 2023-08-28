using BanHub.WebCore.Client.Components.Dialogs;
using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.PlayerProfile;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHubData.Mediatr.Commands.Requests.Penalty;
using Humanizer;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace BanHub.WebCore.Client.Components.Tables;

partial class PlayerProfilePenaltyTable
{
    [Parameter] public required Player Player { get; set; }

    [Inject] protected PlayerProfileService PlayerProfileService { get; set; }
    [Inject] protected DialogService DialogService { get; set; }

    private RadzenDataGrid<Penalty> _dataGrid;
    private IEnumerable<Penalty> _data;
    private bool _loading = true;
    private int _totalCount;
    private string? _searchString;

    private async Task LoadData(LoadDataArgs args)
    {
        _loading = true;
        var paginationQuery = new GetProfilePenaltiesPaginationCommand
        {
            Sorts = args.Sorts,
            SearchString = _searchString,
            Top = args.Top ?? 10,
            Skip = args.Skip ?? 0,
            Identity = Player.Identity
        };

        var context = await PlayerProfileService.GetProfilePenaltiesPaginationAsync(paginationQuery);
        _data = context.Data;
        _totalCount = context.Count;
        _loading = false;
    }

    private void OnSearch(string text)
    {
        _searchString = text;
        _dataGrid.Reload();
    }

    private async Task RowClickEvent(DataGridRowMouseEventArgs<Penalty> arg)
    {
        var parameters = new Dictionary<string, object>
        {
            {"Penalty", arg.Data}
        };

        var options = new DialogOptions
        {
            Style = "min-height:auto;min-width:970px;max-height:97%;",
            CloseDialogOnOverlayClick = true
        };

        var title = $"{arg.Data.PenaltyType} - {Player.UserName} - {arg.Data.Submitted.Humanize().Titleize()}";
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
