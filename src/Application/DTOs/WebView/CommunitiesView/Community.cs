namespace BanHub.Application.DTOs.WebView.CommunitiesView;

public class Community
{
    public bool Active { get; set; }
    public Guid CommunityGuid { get; set; }
    public string CommunityName { get; set; }
    public int ServerCount { get; set; }
    public DateTimeOffset HeartBeat { get; set; }
    public DateTimeOffset Created { get; set; }
    public string CommunityWebsite { get; set; }
}
