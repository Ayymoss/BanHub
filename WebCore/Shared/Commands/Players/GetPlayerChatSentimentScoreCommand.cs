using MediatR;

namespace BanHub.WebCore.Shared.Commands.Players;

public class GetPlayerChatSentimentScoreCommand : IRequest<float?>
{
    public string Identity { get; set; }
}
