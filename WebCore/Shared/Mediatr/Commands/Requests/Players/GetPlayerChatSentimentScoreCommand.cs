using MediatR;

namespace BanHub.WebCore.Shared.Mediatr.Commands.Requests.Players;

public class GetPlayerChatSentimentScoreCommand : IRequest<float?>
{
    public string Identity { get; set; }
}
