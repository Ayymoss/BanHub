using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Utilities;
using BanHub.WebCore.Shared.Commands.Players;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Player;

public class GetPlayersPaginationHandler : IRequestHandler<GetPlayersPaginationCommand, PaginationContext<Shared.Models.PlayersView.Player>>
{
    private readonly DataContext _context;

    public GetPlayersPaginationHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<PaginationContext<Shared.Models.PlayersView.Player>> Handle(GetPlayersPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var query = _context.Players.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
            query = query.Where(search =>
                search.CurrentAlias.Alias.IpAddress == request.SearchString ||
                EF.Functions.ILike(search.Identity, $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.CurrentAlias.Alias.UserName, $"%{request.SearchString}%"));

        if (request.Sorts.Any())
            query = request.Sorts.Aggregate(query, (current, sort) => sort.Property switch
            {
                "Identity" => current.ApplySort(sort, p => p.Identity),
                "UserName" => current.ApplySort(sort, p => p.CurrentAlias.Alias.UserName),
                "Penalties" => current.ApplySort(sort, p => p.Penalties.Count),
                "Heartbeat" => current.ApplySort(sort, p => p.Heartbeat),
                "Created" => current.ApplySort(sort, p => p.Created),
                _ => current
            });

        var count = await query.CountAsync(cancellationToken: cancellationToken);
        var pagedData = await query
            .Skip(request.Skip)
            .Take(request.Top)
            .Select(profile => new Shared.Models.PlayersView.Player
            {
                Identity = profile.Identity,
                UserName = profile.CurrentAlias.Alias.UserName,
                Penalties = profile.Penalties.Count,
                Heartbeat = profile.Heartbeat,
                IsOnline = profile.Heartbeat > DateTimeOffset.UtcNow.AddMinutes(-5),
                Created = profile.Created
            }).ToListAsync(cancellationToken: cancellationToken);

        return new PaginationContext<Shared.Models.PlayersView.Player>
        {
            Data = pagedData,
            Count = count
        };
    }
}
