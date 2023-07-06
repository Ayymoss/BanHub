using BanHub.WebCore.Server.Context;
using Data.Enums;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Models.Context;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Services;

public class ServerService : IServerService
{
    private readonly DataContext _context;
    private readonly IStatisticService _statisticService;

    public ServerService(DataContext context, IStatisticService statisticService)
    {
        _context = context;
        _statisticService = statisticService;
    }

    public async Task<ControllerEnums.ReturnState> AddAsync(Data.Domains.Server request)
    {
        var instance = await _context.Instances.FirstOrDefaultAsync(x => x.InstanceGuid == request.Instance!.InstanceGuid);
        if (instance is null) return ControllerEnums.ReturnState.NotFound;

        var server = await _context.Servers.FirstOrDefaultAsync(x => x.ServerId == request.ServerId);
        if (server is not null)
        {
            server.Updated = DateTimeOffset.UtcNow;
            server.ServerName = request.ServerName;
            server.ServerGame = request.ServerGame;
            _context.Servers.Update(server);
            await _context.SaveChangesAsync();
            return ControllerEnums.ReturnState.NoContent;
        }

        var efServer = new EFServer
        {
            ServerId = request.ServerId,
            ServerName = request.ServerName,
            ServerIp = request.ServerIp,
            ServerPort = request.ServerPort,
            InstanceId = instance.Id,
            ServerGame = request.ServerGame,
            Updated = DateTimeOffset.UtcNow
        };

        await _statisticService.UpdateStatisticAsync(ControllerEnums.StatisticType.ServerCount, ControllerEnums.StatisticTypeAction.Add);

        _context.Servers.Add(efServer);
        await _context.SaveChangesAsync();
        return ControllerEnums.ReturnState.Ok;
    }

    public async Task<(ControllerEnums.ReturnState, Data.Domains.Server?)> GetAsync(string serverId)
    {
        var server = await _context.Servers
            .Where(x => x.ServerId == serverId)
            .Select(x => new Data.Domains.Server
            {
                ServerId = x.ServerId,
                ServerName = x.ServerName,
                ServerIp = x.ServerIp,
                ServerPort = x.ServerPort,
                ServerGame = x.ServerGame
            }).FirstOrDefaultAsync();

        return server is null ? (ControllerEnums.ReturnState.NotFound, null) : (ControllerEnums.ReturnState.Ok, server);
    }
}
