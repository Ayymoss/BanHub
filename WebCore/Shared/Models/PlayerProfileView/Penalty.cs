using BanHubData.Enums;

namespace BanHub.WebCore.Shared.Models.PlayerProfileView;

public class Penalty
{
    public Guid PenaltyGuid { get; set; }
    public string IssuerIdentity { get; set; }
    public string IssuerUserName { get; set; }
    public string Reason { get; set; }
    public string? Evidence { get; set; }
    public string CommunityName { get; set; }
    public Guid CommunityGuid { get; set; }
    public DateTimeOffset? Expiration { get; set; }
    public PenaltyType PenaltyType { get; set; }
    public PenaltyScope PenaltyScope { get; set; }
    public PenaltyStatus PenaltyStatus { get; set; }
    public DateTimeOffset Submitted { get; set; }
    public bool Automated { get; set; }
}
