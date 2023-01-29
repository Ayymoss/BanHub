using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Interfaces;

public interface ISearchService
{
    Task<List<SearchDto>?> Search(string query);
}
