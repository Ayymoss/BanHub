namespace BanHub.Application.DTOs.WebView.PlayerProfileView;

public class Chat
{
    public string Message { get; set; }
    public string CommunityName { get; set; }
    public Guid CommunityGuid { get; set; }
    public DateTimeOffset Submitted { get; set; }
    public string ServerId { get; set; }
    public string ServerName { get; set; }
}
