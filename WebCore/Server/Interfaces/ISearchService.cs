using Data.Domains;

namespace BanHub.WebCore.Server.Interfaces;

public interface ISearchService
{
    Task<List<Search>?> SearchAsync(string query);
}
