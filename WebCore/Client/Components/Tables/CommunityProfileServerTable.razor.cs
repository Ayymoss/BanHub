using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Shared.Mediatr.Commands.Community;
using BanHub.WebCore.Shared.Mediatr.Commands.PlayerProfile;
using BanHub.WebCore.Shared.Models.CommunityProfileView;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace BanHub.WebCore.Client.Components.Tables;

partial class CommunityProfileServerTable
{
    [Parameter] public required Community Community { get; set; }

    [Inject] protected CommunityService CommunityService { get; set; }

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
            Sorts = args.Sorts,
            SearchString = _searchString,
            Top = args.Top ?? 10,
            Skip = args.Skip ?? 0,
            Identity = Community.CommunityGuid
        };

        var context = await CommunityService.GetCommunityProfileServersPaginationAsync(paginationQuery);
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
