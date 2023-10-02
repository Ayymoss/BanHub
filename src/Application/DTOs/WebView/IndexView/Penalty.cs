namespace BanHub.Application.DTOs.WebView.IndexView;

public class Penalty
{
    public string CommunityName { get; set; }
    public Guid CommunityGuid { get; set; }
    public string RecipientUserName { get; set; }
    public string RecipientIdentity { get; set; }
    public string Reason { get; set; }
    public DateTimeOffset Submitted { get; set; }
}
