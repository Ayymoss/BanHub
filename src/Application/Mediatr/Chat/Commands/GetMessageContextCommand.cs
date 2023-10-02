using BanHub.Application.DTOs.WebView.PlayerProfileView;
using MediatR;

namespace BanHub.Application.Mediatr.Chat.Commands;

public class GetMessageContextCommand : IRequest<ChatContextRoot>
{
    public DateTimeOffset Submitted { get; set; }
    public string ServerId { get; set; }
}
