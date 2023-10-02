using BanHub.Application.Mediatr.Services.Events.Statistics;
using BanHub.Domain.Entities;
using BanHub.Domain.Enums;
using BanHub.Domain.Interfaces.Repositories;
using MediatR;

namespace BanHub.Application.Mediatr.Server.Commands;

// TODO: Make this a Notification. Validation should be done separately.

public class CreateOrUpdateServerHandler(IPublisher publisher, ICommunityRepository communityRepository, IServerRepository serverRepository)
    : IRequestHandler<CreateOrUpdateServerCommand, ControllerEnums.ReturnState>
{
    public async Task<ControllerEnums.ReturnState> Handle(CreateOrUpdateServerCommand request, CancellationToken cancellationToken)
    {
        var communityId = await communityRepository.GetCommunityIdAsync(request.CommunityGuid, cancellationToken);
        if (communityId is null) return ControllerEnums.ReturnState.NotFound;

        var server = await serverRepository.GetServerAsync(request.ServerId, cancellationToken);
        if (server is not null)
        {
            server.Updated = DateTimeOffset.UtcNow;
            server.ServerName = request.ServerName;
            server.ServerGame = request.ServerGame;
            await serverRepository.AddOrUpdateServerAsync(server, cancellationToken);
            return ControllerEnums.ReturnState.NoContent;
        }

        server = new EFServer
        {
            ServerId = request.ServerId,
            ServerName = request.ServerName,
            ServerIp = request.ServerIp,
            ServerPort = request.ServerPort,
            CommunityId = communityId.Value,
            ServerGame = request.ServerGame,
            Updated = DateTimeOffset.UtcNow
        };

        await publisher.Publish(new UpdateStatisticsNotification
        {
            StatisticType = ControllerEnums.StatisticType.ServerCount,
            StatisticTypeAction = ControllerEnums.StatisticTypeAction.Add
        }, cancellationToken);

        await serverRepository.AddOrUpdateServerAsync(server, cancellationToken);
        return ControllerEnums.ReturnState.Ok;
    }
}
