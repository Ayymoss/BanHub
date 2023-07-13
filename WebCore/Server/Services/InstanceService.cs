using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Domains;
using BanHubData.Enums;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BanHub.WebCore.Server.Services;

public class InstanceService : IInstanceService
{
    private readonly DataContext _context;
    private readonly ApiKeyCache _apiKeyCache;

    public InstanceService(DataContext context, ApiKeyCache apiKeyCache)
    {
        _context = context;
        _apiKeyCache = apiKeyCache;

    }
    

    public async Task<(ControllerEnums.ReturnState, Instance?)> GetInstanceAsync(string guid)
    {
        var guidParse = Guid.TryParse(guid, out var guidResult);
        if (!guidParse) return (ControllerEnums.ReturnState.BadRequest, null);

        var result = await _context.Instances
            .Where(x => x.InstanceGuid == guidResult)
            .Select(x => new Instance
            {
                InstanceGuid = x.InstanceGuid,
                InstanceIp = x.InstanceIp,
                InstanceName = x.InstanceName,
                About = x.About,
                Socials = x.Socials,
                Active = x.Active,
                HeartBeat = x.HeartBeat,
                Created = x.Created,
                Servers = x.ServerConnections
                    .Where(srv => srv.Instance.InstanceGuid == x.InstanceGuid)
                    .Select(srv => new BanHubData.Domains.Server
                    {
                        ServerId = srv.ServerId,
                        ServerName = srv.ServerName,
                        ServerIp = srv.ServerIp,
                        ServerPort = srv.ServerPort,
                        ServerGame = srv.ServerGame,
                        Updated = srv.Updated
                    }).ToList()
            }).FirstOrDefaultAsync();

        return result is null ? (ControllerEnums.ReturnState.NotFound, null) : (ControllerEnums.ReturnState.Ok, result);
    }

    public async Task<List<Instance>> PaginationAsync(Pagination pagination)
    {
        var query = _context.Instances.AsQueryable();

        if (!string.IsNullOrWhiteSpace(pagination.SearchString))
        {
            query = query.Where(search =>
                EF.Functions.ILike(search.InstanceGuid.ToString(), $"%{pagination.SearchString}%") ||
                EF.Functions.ILike(search.InstanceName ?? "Unknown", $"%{pagination.SearchString}%") ||
                EF.Functions.ILike(search.InstanceIp, $"%{pagination.SearchString}%"));
        }

        query = pagination.SortLabel switch
        {
            "Id" => query.OrderByDirection((SortDirection)pagination.SortDirection!, key => key.Id),
            "Instance Name" => query.OrderByDirection((SortDirection)pagination.SortDirection!, key => key.InstanceName),
            "Instance IP" => query.OrderByDirection((SortDirection)pagination.SortDirection!, key => key.InstanceIp),
            "Heart Beat" => query.OrderByDirection((SortDirection)pagination.SortDirection!, key => key.HeartBeat),
            "Created" => query.OrderByDirection((SortDirection)pagination.SortDirection!, key => key.Created),
            "Servers" => query.OrderByDirection((SortDirection)pagination.SortDirection!, key => key.ServerConnections.Count),
            _ => query
        };

        var pagedData = await query
            .Skip(pagination.Page * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(instance => new Instance
            {
                Active = instance.Active,
                InstanceGuid = instance.InstanceGuid,
                InstanceIp = instance.InstanceIp,
                InstanceName = instance.InstanceName,
                HeartBeat = instance.HeartBeat,
                Created = instance.Created,
                ServerCount = instance.ServerConnections.Count
            }).ToListAsync();

        return pagedData;
    }
}
