using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Shared.Commands.PlayerProfile;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace BanHub.WebCore.Client.Components.Tables;

partial class PlayerProfileConnectionTable
{
    [Parameter] public required Player Player { get; set; }

    [Inject] protected PlayerProfileService PlayerProfileService { get; set; }

    private RadzenDataGrid<Connection> _dataGrid;
    private IEnumerable<Connection> _data;
    private bool _loading = true;
    private int _totalCount;
    private string? _searchString;

    private async Task LoadData(LoadDataArgs args)
    {
        _loading = true;
        var paginationQuery = new GetProfileConnectionsPaginationCommand
        {
            Sorts = args.Sorts,
            SearchString = _searchString,
            Top = args.Top ?? 10,
            Skip = args.Skip ?? 0,
            Identity = Player.Identity
        };

        var context = await PlayerProfileService.GetProfileConnectionsPaginationAsync(paginationQuery);
        _data = context.Data;
        _totalCount = context.Count;
        _loading = false;
    }

    private void OnSearch(string text)
    {
        _searchString = text;
        _dataGrid.Reload();
    }
}
