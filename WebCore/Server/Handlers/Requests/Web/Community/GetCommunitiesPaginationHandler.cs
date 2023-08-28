using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Utilities;
using BanHub.WebCore.Shared.Commands.Community;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Requests.Web.Community;

public class GetCommunitiesPaginationHandler : IRequestHandler<GetCommunitiesPaginationCommand,
    PaginationContext<Shared.Models.CommunitiesView.Community>>
{
    private readonly DataContext _context;

    public GetCommunitiesPaginationHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<PaginationContext<Shared.Models.CommunitiesView.Community>> Handle(GetCommunitiesPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var query = request.Privileged
            ? _context.Communities.AsQueryable()
            : _context.Communities.Where(x => x.Active).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
            query = query.Where(search =>
                EF.Functions.ILike(search.CommunityGuid.ToString(), $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.CommunityName, $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.CommunityIp, $"%{request.SearchString}%"));

        if (request.Sorts.Any())
            query = request.Sorts.Aggregate(query, (current, sort) => sort.Property switch
            {
                "CommunityGuid" => current.ApplySort(sort, p => p.Id),
                "CommunityName" => current.ApplySort(sort, p => p.CommunityName),
                "CommunityWebsite" => current.ApplySort(sort, p => p.CommunityIpFriendly ?? p.CommunityIp),
                "HeartBeat" => current.ApplySort(sort, p => p.HeartBeat),
                "Created" => current.ApplySort(sort, p => p.Created),
                "ServerCount" => current.ApplySort(sort, p => p.ServerConnections.Count),
                _ => current
            });

        var count = await query.CountAsync(cancellationToken: cancellationToken);

        var pagedData = await query
            .Skip(request.Skip)
            .Take(request.Top)
            .Select(instance => new Shared.Models.CommunitiesView.Community
            {
                Active = instance.Active,
                CommunityGuid = instance.CommunityGuid,
                CommunityWebsite = instance.CommunityIpFriendly ?? $"{instance.CommunityIp}:{instance.CommunityPort}",
                CommunityName = instance.CommunityName,
                HeartBeat = instance.HeartBeat,
                Created = instance.Created,
                ServerCount = instance.ServerConnections.Count
            }).ToListAsync(cancellationToken: cancellationToken);

        return new PaginationContext<Shared.Models.CommunitiesView.Community>
        {
            Data = pagedData,
            Count = count
        };
    }
}
