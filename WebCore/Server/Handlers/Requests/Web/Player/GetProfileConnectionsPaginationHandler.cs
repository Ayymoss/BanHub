using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Utilities;
using BanHub.WebCore.Shared.Commands.PlayerProfile;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Requests.Web.Player;

public class GetProfileConnectionsPaginationHandler : IRequestHandler<GetProfileConnectionsPaginationCommand, PaginationContext<Connection>>
{
    private readonly DataContext _context;

    public GetProfileConnectionsPaginationHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<PaginationContext<Connection>> Handle(GetProfileConnectionsPaginationCommand request, CancellationToken cancellationToken)
    {
        var query = _context.ServerConnections
            .Where(x => x.Player.Identity == request.Identity)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
            query = query.Where(search =>
                EF.Functions.ILike(search.Server.Community.CommunityName, $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.Server.ServerName, $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.Server.ServerGame.ToString(), $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.Server.ServerIp, $"%{request.SearchString}%"));

        if (request.Sorts.Any())
            query = request.Sorts.Aggregate(query, (current, sort) => sort.Property switch
            {
                "CommunityName" => current.ApplySort(sort, p => p.Server.Community.CommunityName),
                "ServerName" => current.ApplySort(sort, p => p.Server.ServerName),
                "ServerGame" => current.ApplySort(sort, p => p.Server.ServerGame),
                "ServerIp" => current.ApplySort(sort, p => p.Server.ServerIp),
                "Connected" => current.ApplySort(sort, p => p.Connected),
                _ => current
            });

        var count = await query.CountAsync(cancellationToken: cancellationToken);
        var pagedData = await query
            .Skip(request.Skip)
            .Take(request.Top)
            .Select(x => new Connection
            {
                ServerName = x.Server.ServerName,
                ServerGame = x.Server.ServerGame,
                Connected = x.Connected,
                ServerPort = x.Server.ServerPort,
                ServerIp = x.Server.ServerIp,
                CommunityIp = x.Server.Community.CommunityIp,
                CommunityName = x.Server.Community.CommunityName,
                CommunityGuid = x.Server.Community.CommunityGuid
            }).ToListAsync(cancellationToken: cancellationToken);

        return new PaginationContext<Connection>
        {
            Data = pagedData,
            Count = count
        };
    
        
    }
}
