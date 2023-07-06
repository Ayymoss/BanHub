using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Interfaces;
using Data.Domains;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Services;

public class SearchService : ISearchService
{
    private readonly DataContext _context;

    public SearchService(DataContext context)
    {
        _context = context;
    }

    public async Task<List<Search>?> SearchAsync(string query)
    {
        var result = await _context.Players
            .Where(x => x.CurrentAlias.Alias.UserName.Contains(query) || x.Identity.Contains(query))
            .Select(
                x => new Search
                {
                    Identity = x.Identity,
                    Username = x.CurrentAlias.Alias.UserName
                }).ToListAsync();

        return result;
    }
}
