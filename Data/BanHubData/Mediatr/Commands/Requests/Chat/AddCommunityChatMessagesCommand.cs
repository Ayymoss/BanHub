using MediatR;

namespace BanHubData.Mediatr.Commands.Requests.Chat;

public class AddCommunityChatMessagesCommand : IRequest
{
    public Guid CommunityGuid { get; set; }
    public Dictionary<string, List<MessageContext>> PlayerMessages { get; set; }
}

public record MessageContext(DateTimeOffset Submitted, string ServerId, string Message);
