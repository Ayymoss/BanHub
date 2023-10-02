namespace BanHub.Domain.ValueObjects.Repository.Community;

public class CommunityView
{
    public Guid CommunityGuid { get; set; }
    public string CommunityName { get; set; }
    public string CommunityIp { get; set; }
    public int CommunityPort { get; set; }
    public string? CommunityIpFriendly { get; set; }
    public DateTimeOffset HeartBeat { get; set; }
    public bool Active { get; set; }
    public string? About { get; set; }
    public Dictionary<string, string>? Socials { get; set; }
    public DateTimeOffset Created { get; set; }
}
