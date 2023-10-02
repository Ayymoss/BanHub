using BanHub.Application.DTOs.WebView.PlayerProfileView;
using BanHub.Domain.Interfaces.Repositories;
using MediatR;

namespace BanHub.Application.Mediatr.Chat.Commands;

public class GetChatCountHandler(IChatRepository chatRepository)
    : IRequestHandler<GetChatCountCommand, ChatCount>
{
    public async Task<ChatCount> Handle(GetChatCountCommand request, CancellationToken cancellationToken)
    {
        var count = await chatRepository.GetPlayerChatCountAsync(request.PlayerIdentity, cancellationToken);
        return new ChatCount {Count = count};
    }
}
