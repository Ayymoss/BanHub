using MediatR;

namespace BanHubData.Commands.Chat;

public class AddCommunityChatMessagesCommand : IRequest
{
    public Guid CommunityGuid { get; set; }
    public Dictionary<string, List<(DateTimeOffset Submitted, string Message)>> PlayerMessages { get; set; }
}
