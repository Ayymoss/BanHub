using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Commands.Search;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Search;

public class GetSearchHandler : IRequestHandler<GetSearchCommand, IEnumerable<Shared.Models.SearchView.Search>>
{
    private readonly DataContext _context;

    public GetSearchHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Shared.Models.SearchView.Search>> Handle(GetSearchCommand request, CancellationToken cancellationToken)
    {
        var result = await _context.Players
            .Where(x => x.CurrentAlias.Alias.UserName.Contains(request.Query) || x.Identity.Contains(request.Query))
            .Select(x => new Shared.Models.SearchView.Search()
            {
                Identity = x.Identity,
                Username = x.CurrentAlias.Alias.UserName
            }).ToListAsync(cancellationToken: cancellationToken);

        return result;
    }
}
