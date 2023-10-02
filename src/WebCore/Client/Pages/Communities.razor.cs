using BanHub.Application.DTOs.WebView.CommunitiesView;
using BanHub.Application.Mediatr.Community.Commands;
using BanHub.Domain.Enums;
using BanHub.WebCore.Client.Services.RestEase.Pages;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using SortDescriptor = BanHub.Domain.ValueObjects.Services.SortDescriptor;

namespace BanHub.WebCore.Client.Pages;

partial class Communities(CommunityService communityService, TooltipService tooltipService)
{
    private RadzenDataGrid<Community> _dataGrid;
    private IEnumerable<Community> _playerTable;
    private bool _isLoading = true;
    private int _count;
    private string? _searchString;

    private async Task LoadData(LoadDataArgs args)
    {
        _isLoading = true;

        var paginationQuery = new GetCommunitiesPaginationCommand
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

        var context = await communityService.GetCommunitiesPaginationAsync(paginationQuery);
        _playerTable = context.Data;
        _count = context.Count;
        _isLoading = false;
    }

    private void OnSearch(string text)
    {
        _searchString = text;
        _dataGrid.Reload();
    }

    private void ShowTooltip(ElementReference elementReference, TooltipOptions? options, string message) =>
        tooltipService.Open(elementReference, message, options);
}
