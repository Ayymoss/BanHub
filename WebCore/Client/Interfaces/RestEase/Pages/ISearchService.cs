using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase.Pages;

public interface ISearchService
{
    [Get("/Search/{query}")]
    Task<HttpResponseMessage> GetSearchResultsAsync([Query("query")] string query);
}
