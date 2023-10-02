using BanHub.Application.DTOs.WebView.PlayersView;
using BanHub.Application.Mediatr.Player.Commands;
using BanHub.Domain.Enums;
using BanHub.WebCore.Client.Services.RestEase.Pages;
using Radzen;
using Radzen.Blazor;
using SortDescriptor = BanHub.Domain.ValueObjects.Services.SortDescriptor;

namespace BanHub.WebCore.Client.Pages;

partial class Players(PlayersService playersService)
{
    private RadzenDataGrid<Player> _dataGrid;
    private IEnumerable<Player> _playerTable;
    private bool _isLoading = true;
    private int _count;
    private string? _searchString;

    private async Task LoadData(LoadDataArgs args)
    {
        _isLoading = true;
        var paginationQuery = new GetPlayersPaginationCommand
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

        var context = await playersService.GetPlayersPaginationAsync(paginationQuery);
        _playerTable = context.Data;
        _count = context.Count;
        _isLoading = false;
    }

    private void OnSearch(string text)
    {
        _searchString = text;
        _dataGrid.Reload();
    }
}
