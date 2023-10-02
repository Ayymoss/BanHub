using BanHub.Application.DTOs.WebView.PlayerProfileView;
using MediatR;

namespace BanHub.Application.Mediatr.Chat.Commands;

public class GetChatCountCommand : IRequest<ChatCount>
{
    public string PlayerIdentity { get; set; }
}
