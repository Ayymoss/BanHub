using BanHub.Application.DTOs.WebView.PlayersView;
using BanHub.Application.Mediatr.Player.Commands;
using BanHub.Domain.Interfaces.Repositories.Pagination;
using BanHub.Domain.ValueObjects.Services;
using BanHub.Infrastructure.Context;
using BanHub.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BanHub.Infrastructure.Repositories.Pagination;

public class PlayersPaginationQueryHelper(DataContext context) : IResourceQueryHelper<GetPlayersPaginationCommand, Player>
{
    public async Task<PaginationContext<Player>> QueryResourceAsync(GetPlayersPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var query = context.Players.AsQueryable();

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
            .Select(profile => new Player
            {
                Identity = profile.Identity,
                UserName = profile.CurrentAlias.Alias.UserName,
                Penalties = profile.Penalties.Count,
                Heartbeat = profile.Heartbeat,
                IsOnline = profile.Heartbeat > DateTimeOffset.UtcNow.AddMinutes(-5),
                Created = profile.Created
            }).ToListAsync(cancellationToken: cancellationToken);

        return new PaginationContext<Player>
        {
            Data = pagedData,
            Count = count
        };
    }
}
