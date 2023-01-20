using GlobalInfraction.WebCore.Server.Context;
using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Server.Interfaces;
using GlobalInfraction.WebCore.Server.Models;
using GlobalInfraction.WebCore.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GlobalInfraction.WebCore.Server.Services;

public class ServerService : IServerService
{
    private readonly DataContext _context;
    private readonly IStatisticService _statisticService;

    public ServerService(DataContext context, IStatisticService statisticService)
    {
        _context = context;
        _statisticService = statisticService;
    }

    public async Task<ControllerEnums.ProfileReturnState> Add(ServerDto request)
    {
        var instance = await _context.Instances.FirstOrDefaultAsync(x => x.InstanceGuid == request.Instance!.InstanceGuid);
        if (instance is null) return ControllerEnums.ProfileReturnState.NotFound;

        var server = await _context.Servers.FirstOrDefaultAsync(x => x.ServerId == request.ServerId);
        if (server is not null) return ControllerEnums.ProfileReturnState.Conflict;

        var efServer = new EFServer
        {
            ServerId = request.ServerId,
            ServerName = request.ServerName!,
            ServerIp = request.ServerIp!,
            ServerPort = request.ServerPort!.Value,
            InstanceId = instance.Id,
        };

        await _statisticService.UpdateStatistic(ControllerEnums.StatisticType.ServerCount);
        
        _context.Servers.Add(efServer);
        await _context.SaveChangesAsync();
        return ControllerEnums.ProfileReturnState.Ok;
    }

    public async Task<(ControllerEnums.ProfileReturnState, ServerDto?)> Get(string serverId)
    {
        var server = await _context.Servers
            .Where(x => x.ServerId == serverId)
            .Select(x => new ServerDto
            {
                ServerId = x.ServerId,
                ServerName = x.ServerName,
                ServerIp = x.ServerIp,
                ServerPort = x.ServerPort
            })
            .FirstOrDefaultAsync();

        return server is null ? (ControllerEnums.ProfileReturnState.NotFound, null) : (ControllerEnums.ProfileReturnState.Ok, server);
    }
}
