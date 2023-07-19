using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Models.Domains;
using BanHub.WebCore.Shared.Commands.Instance;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BanHub.WebCore.Server.Handlers.Web.Instance;

public class GetInstancesPaginationHandler : IRequestHandler<GetInstancesPaginationCommand,
    IEnumerable<Shared.Models.InstancesView.Instance>>
{
    private readonly DataContext _context;

    public GetInstancesPaginationHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Shared.Models.InstancesView.Instance>> Handle(GetInstancesPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var query = request.Privileged 
            ? _context.Instances.AsQueryable() 
            : _context.Instances.Where(x=>x.Active).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            query = query.Where(search =>
                EF.Functions.ILike(search.InstanceGuid.ToString(), $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.InstanceName, $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.InstanceIp, $"%{request.SearchString}%"));
        }

        query = request.SortLabel switch
        {
            "Id" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.Id),
            "Name" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.InstanceName),
            "Website" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.InstanceIpFriendly ?? key.InstanceIp),
            "HeartBeat" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.HeartBeat),
            "Created" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.Created),
            "Servers" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.ServerConnections.Count),
            _ => query
        };

        var pagedData = await query
            .Skip(request.Page * request.PageSize)
            .Take(request.PageSize)
            .Select(instance => new Shared.Models.InstancesView.Instance
            {
                Active = instance.Active,
                InstanceGuid = instance.InstanceGuid,
                InstanceWebsite = instance.InstanceIpFriendly ?? $"{instance.InstanceIp}:{instance.InstancePort}",
                InstanceName = instance.InstanceName,
                HeartBeat = instance.HeartBeat,
                Created = instance.Created,
                ServerCount = instance.ServerConnections.Count
            }).ToListAsync(cancellationToken: cancellationToken);

        return pagedData;
    }
}
