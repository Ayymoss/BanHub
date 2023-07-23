namespace BanHub.WebCore.Shared.Models.CommunitiesView;

public class CommunityContext
{
    public List<Community> Communities { get; set; }
    public int Count { get; set; }
}
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
