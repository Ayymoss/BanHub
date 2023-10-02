using BanHub.Domain.Enums;

namespace BanHub.Domain.ValueObjects.Plugin;

public class AddPlayerPenaltyCommandSlim
{
    public PenaltyType PenaltyType { get; set; }
    public PenaltyScope PenaltyScope { get; set; }
    public string Reason { get; set; }
    public bool Automated{ get; set; }
    public DateTimeOffset? Expiration { get; set; }
    public string AdminIdentity { get; set; }
    public string TargetIdentity { get; set; }
    public Guid CommunityGuid { get; set; }
}
