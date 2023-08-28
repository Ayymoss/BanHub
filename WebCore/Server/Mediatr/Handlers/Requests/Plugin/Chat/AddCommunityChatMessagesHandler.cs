using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Models.Domains;
using BanHubData.Mediatr.Commands.Requests.Chat;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Plugin.Chat;

public class AddCommunityChatMessagesHandler : IRequestHandler<AddCommunityChatMessagesCommand>
{
    private readonly DataContext _context;
    private readonly ILogger<AddCommunityChatMessagesHandler> _logger;

    public AddCommunityChatMessagesHandler(DataContext context, ILogger<AddCommunityChatMessagesHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(AddCommunityChatMessagesCommand request, CancellationToken cancellationToken)
    {
        var community = await _context.Communities
            .Where(x => x.CommunityGuid == request.CommunityGuid)
            .Select(x => new {x.Id})
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (community is null) return;

        var players = await _context.Players
            .Where(x => request.PlayerMessages.Keys.Contains(x.Identity))
            .Select(x => new
            {
                x.Id,
                x.Identity
            }).ToListAsync(cancellationToken: cancellationToken);

        var serverIds = request.PlayerMessages.Values
            .SelectMany(y => y)
            .Select(y => y.ServerId)
            .ToList();

        var servers = await _context.Servers
            .Where(x => serverIds.Contains(x.ServerId))
            .Select(x => new
            {
                x.Id,
                x.ServerId
            }).ToListAsync(cancellationToken: cancellationToken);

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
                CommunityId = community.Id
            }).ToList();

            await _context.Chats.AddRangeAsync(chatList, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
