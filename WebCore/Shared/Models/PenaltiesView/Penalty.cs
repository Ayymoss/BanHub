using BanHubData.Enums;

namespace BanHub.WebCore.Shared.Models.PenaltiesView;

public class PenaltyContext
{
    public List<Penalty> Penalties { get; set; } = new();
    public int Count { get; set; }
}

public class Penalty
{
    public string TargetIdentity { get; set; }
    public string TargetUserName { get; set; }
    public string AdminIdentity { get; set; }
    public Guid PenaltyGuid { get; set; }
    public string AdminUserName { get; set; }
    public string Reason { get; set; }
    public string? Evidence { get; set; }
    public string CommunityName { get; set; }
    public Guid CommunityGuid { get; set; }
    public DateTimeOffset? Expiration { get; set; }
    public PenaltyType PenaltyType { get; set; }
    public PenaltyScope PenaltyScope { get; set; }
    public PenaltyStatus PenaltyStatus { get; set; }
    public DateTimeOffset Submitted { get; set; }
}
