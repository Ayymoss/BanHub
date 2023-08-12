using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Models.Domains;
using BanHubData.Commands.Community.Server;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Plugin.Community.Server;

public class CreateOrUpdateServerHandler : IRequestHandler<CreateOrUpdateServerCommand, ControllerEnums.ReturnState>
{
    private readonly DataContext _context;
    private readonly IStatisticService _statisticService;

    public CreateOrUpdateServerHandler(DataContext context, IStatisticService statisticService)
    {
        _context = context;
        _statisticService = statisticService;
    }

    public async Task<ControllerEnums.ReturnState> Handle(CreateOrUpdateServerCommand request, CancellationToken cancellationToken)
    {
        var instance = await _context.Communities
            .FirstOrDefaultAsync(x => x.CommunityGuid == request.CommunityGuid, cancellationToken: cancellationToken);
        if (instance is null) return ControllerEnums.ReturnState.NotFound;

        var server = await _context.Servers
            .FirstOrDefaultAsync(x => x.ServerId == request.ServerId, cancellationToken: cancellationToken);
        if (server is not null)
        {
            server.Updated = DateTimeOffset.UtcNow;
            server.ServerName = request.ServerName;
            server.ServerGame = request.ServerGame;
            _context.Servers.Update(server);
            await _context.SaveChangesAsync(cancellationToken);
            return ControllerEnums.ReturnState.NoContent;
        }

        var efServer = new EFServer
        {
            ServerId = request.ServerId,
            ServerName = request.ServerName,
            ServerIp = request.ServerIp,
            ServerPort = request.ServerPort,
            CommunityId = instance.Id,
            ServerGame = request.ServerGame,
            Updated = DateTimeOffset.UtcNow
        };

        await _statisticService.UpdateStatisticAsync(ControllerEnums.StatisticType.ServerCount, ControllerEnums.StatisticTypeAction.Add);

        _context.Servers.Add(efServer);
        await _context.SaveChangesAsync(cancellationToken);
        return ControllerEnums.ReturnState.Ok;
    }
}
