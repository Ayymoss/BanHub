using MediatR;

namespace BanHub.WebCore.Server.Mediatr.Commands.Requests;

public class CalculateChatSentimentCommand : IRequest<float>
{
    public IEnumerable<Message> Messages { get; set; }
}

public record Message(string Comment);
