namespace BanHub.WebCore.Shared.Models.PlayerProfileView;

public class ChatContextRoot
{
    public bool Loaded { get; set; }
    public IEnumerable<ChatContext> Messages { get; set; }
}

public class ChatContext
{
    public DateTimeOffset Submitted { get; set; }
    public string PlayerUserName { get; set; }
    public string PlayerIdentity { get; set; }
    public string Message { get; set; }
}
