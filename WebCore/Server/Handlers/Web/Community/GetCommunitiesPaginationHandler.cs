using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Commands.Community;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BanHub.WebCore.Server.Handlers.Web.Community;

public class GetCommunitiesPaginationHandler : IRequestHandler<GetCommunitiesPaginationCommand,
    IEnumerable<Shared.Models.CommunitiesView.Community>>
{
    private readonly DataContext _context;

    public GetCommunitiesPaginationHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Shared.Models.CommunitiesView.Community>> Handle(GetCommunitiesPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var query = request.Privileged 
            ? _context.Communities.AsQueryable() 
            : _context.Communities.Where(x=>x.Active).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            query = query.Where(search =>
                EF.Functions.ILike(search.CommunityGuid.ToString(), $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.CommunityName, $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.CommunityIp, $"%{request.SearchString}%"));
        }

        query = request.SortLabel switch
        {
            "Id" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.Id),
            "Name" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.CommunityName),
            "Website" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.CommunityIpFriendly ?? key.CommunityIp),
            "HeartBeat" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.HeartBeat),
            "Created" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.Created),
            "Servers" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.ServerConnections.Count),
            _ => query
        };

        var pagedData = await query
            .Skip(request.Page * request.PageSize)
            .Take(request.PageSize)
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

        return pagedData;
    }
}
