using BanHub.Application.DTOs.WebView.CommunityProfileView;
using BanHub.Application.Mediatr.Community.Commands;
using BanHub.Domain.Enums;
using BanHub.WebCore.Client.Services.RestEase.Pages;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using SortDescriptor = BanHub.Domain.ValueObjects.Services.SortDescriptor;

namespace BanHub.WebCore.Client.Components.Tables;

partial class CommunityProfileServerTable(CommunityService communityService)
{
    [Parameter] public required Community Community { get; set; }

    private RadzenDataGrid<Server> _dataGrid;
    private IEnumerable<Server> _data;
    private bool _loading = true;
    private int _totalCount;
    private string? _searchString;

    private async Task LoadData(LoadDataArgs args)
    {
        _loading = true;
        var paginationQuery = new GetCommunityProfileServersPaginationCommand
        {
            Sorts = args.Sorts.Select(x => new SortDescriptor
            {
                Property = x.Property,
                SortOrder = x.SortOrder == SortOrder.Ascending
                    ? SortDirection.Ascending
                    : SortDirection.Descending
            }),
            SearchString = _searchString,
            Top = args.Top ?? 10,
            Skip = args.Skip ?? 0,
            Identity = Community.CommunityGuid
        };

        var context = await communityService.GetCommunityProfileServersPaginationAsync(paginationQuery);
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
