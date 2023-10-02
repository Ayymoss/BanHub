using BanHub.Domain.Entities;
using BanHub.Domain.Interfaces.Repositories;
using MediatR;

namespace BanHub.Application.Mediatr.Chat.Events;

public class AddCommunityChatMessagesHandler(ICommunityRepository communityRepository, IPlayerRepository playerRepository,
    IServerRepository serverRepository, IChatRepository chatRepository) : INotificationHandler<AddCommunityChatMessagesNotification>
{
    public async Task Handle(AddCommunityChatMessagesNotification request, CancellationToken cancellationToken)
    {
        var community = await communityRepository.GetCommunityIdAsync(request.CommunityGuid, cancellationToken);
        if (community is null) return;

        var serverIds = request.PlayerMessages.Values
            .SelectMany(y => y)
            .Select(y => y.ServerId)
            .ToList();

        var players = (await playerRepository.GetPlayerIdentitiesAsync(request.PlayerMessages.Keys, cancellationToken)).ToArray();
        var servers = (await serverRepository.GetServerIdentifiersAsync(serverIds, cancellationToken)).ToArray();

        foreach (var playerMessage in request.PlayerMessages)
        {
            var playerId = players.FirstOrDefault(x => x.Identity == playerMessage.Key)?.Id;
            var serverId = servers.FirstOrDefault(x => x.ServerId == playerMessage.Value.FirstOrDefault()?.ServerId)?.Id;
            if (!playerId.HasValue || !serverId.HasValue) continue;

            var chatList = playerMessage.Value.Select(value => new EFChat
            {
                Message = value.Message,
                Submitted = value.Submitted,
                PlayerId = playerId.Value,
                ServerId = serverId.Value,
                CommunityId = community.Value
            }).ToList();

            await chatRepository.AddChatsAsync(chatList, cancellationToken);
        }
    }
}
