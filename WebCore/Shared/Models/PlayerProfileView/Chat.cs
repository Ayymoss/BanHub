namespace BanHub.WebCore.Shared.Models.PlayerProfileView;

public class Chat
{
    public string Message { get; set; }
    public string CommunityName { get; set; }
    public Guid CommunityGuid { get; set; }
    public DateTimeOffset Submitted { get; set; }
}
