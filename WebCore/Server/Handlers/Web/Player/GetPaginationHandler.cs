using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Commands.Players;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BanHub.WebCore.Server.Handlers.Web.Player;

public class GetPaginationHandler : IRequestHandler<GetPlayersPaginationCommand, IEnumerable<Shared.Models.PlayersView.Player>>
{
    private readonly DataContext _context;

    public GetPaginationHandler(DataContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<Shared.Models.PlayersView.Player>> Handle(GetPlayersPaginationCommand request, CancellationToken cancellationToken)
    {
        var query = _context.Players.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            query = query.Where(search =>
                EF.Functions.ILike(search.Identity, $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.CurrentAlias.Alias.UserName, $"%{request.SearchString}%"));
        }

        query = request.SortLabel switch
        {
            "Id" => query.OrderByDirection((SortDirection)request.SortDirection, entity => entity.Identity),
            "Name" => query.OrderByDirection((SortDirection)request.SortDirection, entity => entity.CurrentAlias.Alias.UserName),
            "Penalty" => query.OrderByDirection((SortDirection)request.SortDirection, entity => entity.Penalties.Count),
            "Online" => query.OrderByDirection((SortDirection)request.SortDirection, entity => entity.HeartBeat),
            "Created" => query.OrderByDirection((SortDirection)request.SortDirection, entity => entity.Created),
            _ => query
        };

        var pagedData = await query
            .Skip(request.Page * request.PageSize)
            .Take(request.PageSize)
            .Select(profile => new Shared.Models.PlayersView.Player
            {
                Identity = profile.Identity,
                UserName = profile.CurrentAlias.Alias.UserName,
                Penalties = profile.Penalties.Count,
                HeartBeat = profile.HeartBeat,
                Created = profile.Created,
            }).ToListAsync(cancellationToken: cancellationToken);

        return pagedData;
    }
}
