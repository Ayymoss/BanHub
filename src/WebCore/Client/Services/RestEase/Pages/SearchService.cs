using BanHub.Application.DTOs.WebView.SearchView;
using BanHub.WebCore.Client.Interfaces.RestEase;
using BanHub.WebCore.Client.Utilities;
using RestEase;

namespace BanHub.WebCore.Client.Services.RestEase.Pages;

public class SearchService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif
    private readonly ISearchService _api = RestClient.For<ISearchService>(ApiHost);

    public async Task<Search> GetSearchResultsAsync(string query)
    {
        try
        {
            var response = await _api.GetSearchResultsAsync(query);
            var result = await response.DeserializeHttpResponseContentAsync<Search>();
            return result ?? new Search();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get players pagination: {e.Message}");
        }

        return new Search();
    }
}
