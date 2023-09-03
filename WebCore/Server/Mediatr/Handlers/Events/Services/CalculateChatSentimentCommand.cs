using MediatR;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Events.Services;

public class CalculateChatSentimentCommand : IRequest<float>
{
    public IEnumerable<Message> Messages { get; set; }
}

public record Message(string Comment);
