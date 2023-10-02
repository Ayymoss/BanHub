using MediatR;

namespace BanHub.Application.Mediatr.Player.Commands;

public class GetPlayerChatSentimentScoreCommand : IRequest<float?>
{
    public string Identity { get; set; }
}
