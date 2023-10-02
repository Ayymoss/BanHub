namespace BanHub.Domain.ValueObjects.Repository.Penalty;

public class LatestBans
{
    public string CommunityName { get; set; }
    public Guid CommunityGuid { get; set; }
    public string RecipientUserName { get; set; }
    public string RecipientIdentity { get; set; }
    public string Reason { get; set; }
    public bool Automated { get; set; }
    public DateTimeOffset Submitted { get; set; }
}
