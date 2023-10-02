using MediatR;

namespace BanHub.Application.Mediatr.Services.Commands;

public class CalculateChatSentimentCommand : IRequest<float>
{
    public IEnumerable<Message> Messages { get; set; }
}

public record Message(string Comment);
