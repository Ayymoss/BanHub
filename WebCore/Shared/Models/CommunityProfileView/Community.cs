
namespace BanHub.WebCore.Shared.Models.CommunityProfileView;

public class Community
{
    public Guid CommunityGuid { get; set; }
    public string CommunityName { get; set; }
    public string CommunityWebsite { get; set; }
    public bool Connected { get; set; }
    public bool Active { get; set; }
    public string? About { get; set; }
    public Dictionary<string, string>? Socials { get; set; }
    public DateTimeOffset Created { get; set; }
    public int ServerCount { get; set; }
    public int CommunityPort { get; set; }
    public int AutomatedPenaltiesCount { get; set; }
    public int PenaltiesCount { get; set; }
}
