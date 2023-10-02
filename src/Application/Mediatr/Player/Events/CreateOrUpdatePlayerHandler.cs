using BanHub.Application.Mediatr.Services.Events.Statistics;
using BanHub.Application.Utilities;
using BanHub.Domain.Entities;
using BanHub.Domain.Enums;
using BanHub.Domain.Interfaces.Repositories;
using MediatR;

namespace BanHub.Application.Mediatr.Player.Events;

public class CreateOrUpdatePlayerHandler(IPublisher publisher, IPlayerRepository playerRepository, IAliasRepository aliasRepository,
        ICurrentAliasRepository currentAliasRepository, IServerConnectionRepository serverConnectionRepository)
    : INotificationHandler<CreateOrUpdatePlayerNotification>
{
    public async Task Handle(CreateOrUpdatePlayerNotification notification, CancellationToken cancellationToken)
    {
        var utcTimeNow = DateTimeOffset.UtcNow;
        var player = await playerRepository.GetPlayerByIdentityAsync(notification.PlayerIdentity, cancellationToken);

        if (player is null)
        {
            player = new EFPlayer
            {
                Identity = notification.PlayerIdentity,
                Heartbeat = utcTimeNow,
                WebRole = WebRole.User,
                CommunityRole = notification.PlayerCommunityRole,
                Created = utcTimeNow,
                PlayTime = TimeSpan.Zero,
                TotalConnections = 1
            };

            await publisher.Publish(new UpdateStatisticsNotification
            {
                StatisticType = ControllerEnums.StatisticType.PlayerCount,
                StatisticTypeAction = ControllerEnums.StatisticTypeAction.Add
            }, cancellationToken);

            player.Id = await playerRepository.AddOrUpdatePlayerAsync(player, cancellationToken);
        }
        else
        {
            player.Heartbeat = DateTimeOffset.UtcNow;
            player.TotalConnections++;
            player.CommunityRole = notification.PlayerCommunityRole;

            await playerRepository.AddOrUpdatePlayerAsync(player, cancellationToken);
        }

        var alias = await aliasRepository.GetAliasByUserNameAndIpAddressAsync(notification.PlayerAliasUserName,
            notification.PlayerAliasIpAddress, cancellationToken);

        if (alias is null)
        {
            alias = new EFAlias
            {
                UserName = notification.PlayerAliasUserName.FilterUnknownCharacters(),
                IpAddress = notification.PlayerAliasIpAddress,
                Created = DateTimeOffset.UtcNow,
                PlayerId = player.Id
            };

            await aliasRepository.AddAliasAsync(alias);
        }

        var currentAlias = await currentAliasRepository.GetCurrentAliasByPlayerIdAsync(player.Id, cancellationToken);

        if (currentAlias is null || currentAlias.AliasId != alias.Id)
        {
            currentAlias = new EFCurrentAlias
            {
                PlayerId = player.Id,
                AliasId = alias.Id
            };

            await currentAliasRepository.AddOrUpdateCurrentAliasAsync(currentAlias, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(notification.ServerId))
        {
            var serverConnection = new EFServerConnection
            {
                PlayerId = player.Id,
                ServerId = int.Parse(notification.ServerId),
                Connected = DateTimeOffset.UtcNow
            };

            await serverConnectionRepository.AddServerConnectionAsync(serverConnection);
        }
    }
}
