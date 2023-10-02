using BanHub.Application.DTOs.WebView.CommunityProfileView;
using BanHub.Application.Mediatr.Community.Commands;
using BanHub.Application.Utilities;
using BanHub.Domain.Interfaces.Repositories.Pagination;
using BanHub.Domain.ValueObjects.Services;
using BanHub.Infrastructure.Context;
using BanHub.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BanHub.Infrastructure.Repositories.Pagination;

public class CommunityProfileServersPaginationQueryHelper(DataContext context)
    : IResourceQueryHelper<GetCommunityProfileServersPaginationCommand, Server>
{
    public async Task<PaginationContext<Server>> QueryResourceAsync(GetCommunityProfileServersPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var query = context.Servers
            .Where(x => x.Community.CommunityGuid == request.Identity)
            .Where(x => x.Updated < DateTimeOffset.UtcNow.AddMonths(1))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
            query = query.Where(search =>
                EF.Functions.ILike(search.ServerName, $"%{request.SearchString}%") ||
                // EF.Functions.ILike(search.ServerGame.ToString(), $"%{request.SearchString}%") || // TODO: I need a way to accessing the enum as a string. EFCore doesn't like the translation of .ToString()
                EF.Functions.ILike(search.ServerIp, $"%{request.SearchString}%"));

        if (request.Sorts.Any())
            query = request.Sorts.Aggregate(query, (current, sort) => sort.Property switch
            {
                "ServerName" => current.ApplySort(sort, p => p.ServerName),
                "ServerIp" => current.ApplySort(sort, p => p.ServerIp),
                "ServerGame" => current.ApplySort(sort, p => p.ServerGame),
                "Updated" => current.ApplySort(sort, p => p.Updated),
                _ => current
            });

        var count = await query.CountAsync(cancellationToken: cancellationToken);
        var pagedData = await query
            .Skip(request.Skip)
            .Take(request.Top)
            .Select(x => new Server
            {
                ServerName = x.ServerName,
                ServerIp = x.ServerIp.IsInternal() ? x.Community.CommunityIp : x.ServerIp,
                ServerPort = x.ServerPort,
                ServerGame = x.ServerGame,
                Updated = x.Updated,
                ServerId = x.ServerId
            }).ToListAsync(cancellationToken: cancellationToken);

        return new PaginationContext<Server>
        {
            Data = pagedData,
            Count = count
        };
    }
}
