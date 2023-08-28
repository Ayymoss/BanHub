using BanHub.WebCore.Client.Components.Dialogs;
using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.Chat;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.Community;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.PlayerProfile;
using BanHub.WebCore.Shared.Models.CommunityProfileView;
using BanHubData.Mediatr.Commands.Requests.Penalty;
using Humanizer;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace BanHub.WebCore.Client.Components.Tables;

partial class CommunityProfilePenaltyTable
{
    [Parameter] public required Community Community { get; set; }

    [Inject] protected CommunityService CommunityService { get; set; }
    [Inject] protected DialogService DialogService { get; set; }

    private RadzenDataGrid<Penalty> _dataGrid;
    private IEnumerable<Penalty> _data;
    private bool _loading = true;
    private int _totalCount;
    private string? _searchString;

    private async Task LoadData(LoadDataArgs args)
    {
        _loading = true;
        var paginationQuery = new GetCommunityProfilePenaltiesPaginationCommand
        {
            Sorts = args.Sorts,
            SearchString = _searchString,
            Top = args.Top ?? 10,
            Skip = args.Skip ?? 0,
            Identity = Community.CommunityGuid
        };

        var context = await CommunityService.GetCommunityProfilePenaltiesPaginationAsync(paginationQuery);
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
        var convertedPenalty = new BanHub.WebCore.Shared.Models.PlayerProfileView.Penalty
        {
            PenaltyGuid = arg.Data.PenaltyGuid,
            IssuerUserName = arg.Data.IssuerUserName,
            IssuerIdentity = arg.Data.IssuerIdentity,
            Reason = arg.Data.Reason,
            Evidence = arg.Data.Evidence,
            CommunityName = Community.CommunityName,
            CommunityGuid = Community.CommunityGuid,
            Expiration = arg.Data.Expiration,
            PenaltyType = arg.Data.PenaltyType,
            PenaltyScope = arg.Data.PenaltyScope,
            PenaltyStatus = arg.Data.PenaltyStatus,
            Submitted = arg.Data.Submitted,
            Automated = arg.Data.Automated
        };

        var parameters = new Dictionary<string, object>
        {
            {"Penalty", convertedPenalty}
        };

        var options = new DialogOptions
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
