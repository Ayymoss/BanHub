using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase;

public interface ISearchService
{
    [Get("/Search")]
    Task<HttpResponseMessage> GetSearchResultsAsync([Query("query")] string query);
}
