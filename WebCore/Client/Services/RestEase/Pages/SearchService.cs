using BanHub.WebCore.Client.Interfaces.RestEase.Pages;
using BanHub.WebCore.Shared.Models.SearchView;
using BanHub.WebCore.Shared.Utilities;
using RestEase;

namespace BanHub.WebCore.Client.Services.RestEase.Pages;

public class SearchService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif
    private readonly ISearchService _api;

    public SearchService()
    {
        _api = RestClient.For<ISearchService>(ApiHost);
    }

    public async Task<IEnumerable<Search>> GetPlayersAsync(string query)
    {
        try
        {
            var response = await _api.GetSearchResultsAsync(query);
            var result = await response.DeserializeHttpResponseContentAsync<IEnumerable<Search>>();
            return result ?? new List<Search>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get players pagination: {e.Message}");
        }

        return new List<Search>();
    }
}
