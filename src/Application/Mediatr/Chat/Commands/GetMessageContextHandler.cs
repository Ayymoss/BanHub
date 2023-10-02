using BanHub.Application.DTOs.WebView.PlayerProfileView;
using BanHub.Domain.Interfaces.Repositories;
using MediatR;

namespace BanHub.Application.Mediatr.Chat.Commands;

public class GetMessageContextHandler(IChatRepository chatRepository) : IRequestHandler<GetMessageContextCommand, ChatContextRoot>
{
    public async Task<ChatContextRoot> Handle(GetMessageContextCommand request, CancellationToken cancellationToken)
    {
        var chats = await chatRepository.GetPlayerChatContextAsync(request.ServerId, request.Submitted, cancellationToken);

        var result = new ChatContextRoot
        {
            Messages = chats.OrderBy(x => x.Submitted),
            Loaded = true
        };

        return result;
    }
}
