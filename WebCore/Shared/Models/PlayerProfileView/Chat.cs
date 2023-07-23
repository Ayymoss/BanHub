namespace BanHub.WebCore.Shared.Models.PlayerProfileView;

public class ChatContext
{
    public List<Chat> Chats { get; set; } = new();
    public int Count { get; set; }
}

public class Chat
{
    public string Message { get; set; }
    public string CommunityName { get; set; }
    public Guid CommunityGuid { get; set; }
    public DateTimeOffset Submitted { get; set; }
}
