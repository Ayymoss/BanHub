using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Shared.DTOs;

namespace GlobalInfraction.WebCore.Server.Interfaces;

public interface ISearchService
{
    Task<List<SearchDto>?> Search(string query);
}
