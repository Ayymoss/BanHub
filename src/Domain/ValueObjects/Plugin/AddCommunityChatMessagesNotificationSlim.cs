namespace BanHub.Domain.ValueObjects.Plugin;

public class AddCommunityChatMessagesNotificationSlim
{
    public Guid CommunityGuid { get; set; }
    public Dictionary<string, List<MessageContext>> PlayerMessages { get; set; }
}

public record MessageContext(DateTimeOffset Submitted, string ServerId, string Message);
