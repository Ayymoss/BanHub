using BanHub.Application.Utilities;
using BanHub.Domain.Interfaces.Repositories;
using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Server.Commands;

public class GetServersPaginationHandler(ISender mediator, IServerRepository serverRepository)
    : IRequestHandler<GetServersPaginationCommand, PaginationContext<DTOs.WebView.ServersView.Server>>
{
    public async Task<PaginationContext<DTOs.WebView.ServersView.Server>> Handle(GetServersPaginationCommand request,
        CancellationToken cancellationToken)
    {
        // Note, we only expect a small return of servers (<2,500) so we'll prefetch. However, this approach is not scalable.
        // I'm having to do this because we need to get the online counts for each server, and that value is not stored in the database.
        // I don't know how to do this whilst allowing sorting/filtering whilst using IQueryable. 

        var query = await serverRepository.GetServerPaginationAsync(cancellationToken);
        var onlineCounts = await mediator.Send(new GetServerOnlineCountsCommand {ServerIds = query.ServerIds}, cancellationToken);
        var serverViews = query.ServerPaginationView.Select(x => new DTOs.WebView.ServersView.Server
        {
            ServerIp = x.ServerIp.IsInternal() ? x.CommunityIp : x.ServerIp,
            ServerPort = x.ServerPort,
            ServerGame = x.ServerGame,
            ServerName = x.ServerName,
            Updated = x.Updated,
            CommunityGuid = x.CommunityGuid,
            CommunityName = x.CommunityName,
            MaxPlayers = x.MaxPlayers,
            OnlineCount = onlineCounts.TryGetValue(x.ServerIp, out var count) ? count : 0
        }).ToList();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
            serverViews = serverViews
                .Where(s => s.ServerName.Contains(request.SearchString)
                            || s.ServerIp.Contains(request.SearchString)
                            || s.ServerGame.ToString().Contains(request.SearchString)
                            || s.CommunityName.Contains(request.SearchString))
                .ToList();

        if (request.Sorts.Any())
            serverViews = request.Sorts.Aggregate(serverViews, (current, sort) => sort.Property switch
            {
                "OnlineCount" => current.OrderBy(s => s.OnlineCount).ToList(),
                "CommunityName" => current.OrderBy(s => s.CommunityName).ToList(),
                "ServerName" => current.OrderBy(s => s.ServerName).ToList(),
                "ServerIp" => current.OrderBy(s => s.ServerIp).ToList(),
                "ServerGame" => current.OrderBy(s => s.ServerGame).ToList(),
                "Updated" => current.OrderBy(s => s.Updated).ToList(),
                _ => current
            });

        var count = serverViews.Count;
        var pagedData = serverViews
            .Skip(request.Skip)
            .Take(request.Top)
            .ToList();

        return new PaginationContext<DTOs.WebView.ServersView.Server>
        {
            Data = pagedData,
            Count = count
        };
    }
}
