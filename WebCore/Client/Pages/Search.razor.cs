using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Client.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace BanHub.WebCore.Client.Pages;

partial class Search : IAsyncDisposable
{
    [Inject] protected SearchService SearchService { get; set; }
    [Inject] protected NavigationManager NavManager { get; set; }

    private WebCore.Shared.Models.SearchView.Search _searchResults = new();
    private bool _chatEmpty;
    private bool _playerEmpty;
    private bool _badQuery;
    private bool _loading = true;
    private string _query = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await HandleSearch();
        NavManager.LocationChanged += HandleLocationChanged;
    }

    private void HandleLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _ = HandleSearch();
    }

    private async Task HandleSearch()
    {
        NavManager.TryGetQueryString("query", out _query);
        await OnSearch();
        StateHasChanged();
    }

    private async Task OnSearch()
    {
        _loading = true;
        _badQuery = false;
        if (string.IsNullOrEmpty(_query) || _query.Length < 3)
        {
            _badQuery = true;
            return;
        }

        _searchResults = await SearchService.GetSearchResultsAsync(_query);
        _chatEmpty = !_searchResults.Messages.Any();
        _playerEmpty = !_searchResults.Players.Any();
        _loading = false;
    }

    public async ValueTask DisposeAsync()
    {
        NavManager.LocationChanged -= HandleLocationChanged;
        await Task.CompletedTask;
    }
}
