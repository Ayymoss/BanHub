using MediatR;

namespace BanHubData.Commands.Chat;

public class AddCommunityChatMessagesCommand : IRequest
{
    public Guid CommunityGuid { get; set; }
    public Dictionary<string, List<MessageContext>> PlayerMessages { get; set; }
}

public record MessageContext(DateTimeOffset Submitted, string Message);
