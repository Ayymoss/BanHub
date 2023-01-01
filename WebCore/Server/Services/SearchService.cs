using GlobalInfraction.WebCore.Server.Context;
using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Server.Interfaces;
using GlobalInfraction.WebCore.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GlobalInfraction.WebCore.Server.Services;

public class SearchService : ISearchService
{
    private readonly DataContext _context;

    public SearchService(DataContext context)
    {
        _context = context;
    }

    public async Task<List<SearchDto>?> Search(string query)
    {
        var result = await _context.Entities
            .Where(x => x.CurrentAlias.Alias.UserName.Contains(query) || x.Identity.Contains(query))
            .Select(
                x => new SearchDto
                {
                    Identity = x.Identity,
                    Username = x.CurrentAlias.Alias.UserName
                }).ToListAsync();

        return result;
    }
}
