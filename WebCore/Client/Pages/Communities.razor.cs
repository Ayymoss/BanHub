using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Shared.Mediatr.Commands.Community;
using BanHub.WebCore.Shared.Models.CommunitiesView;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace BanHub.WebCore.Client.Pages;

partial class Communities
{
    [Inject] protected CommunityService CommunityService { get; set; }
    [Inject] protected TooltipService TooltipService { get; set; }

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
            Sorts = args.Sorts,
            SearchString = _searchString,
            Top = args.Top ?? 20,
            Skip = args.Skip ?? 0
        };

        var context = await CommunityService.GetCommunitiesPaginationAsync(paginationQuery);
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
        TooltipService.Open(elementReference, message, options);
}
