using BanHubData.Enums;

namespace BanHub.WebCore.Shared.Models.PlayerProfileView;

public class Penalty
{
    public Guid PenaltyGuid { get; set; }
    public string AdminUserName { get; set; }
    public string Reason { get; set; }
    public string? Evidence { get; set; }
    public string? InstanceName { get; set; }
    public Guid InstanceGuid { get; set; }
    public TimeSpan? Duration { get; set; }
    public PenaltyType PenaltyType { get; set; }
    public PenaltyScope PenaltyScope { get; set; }
    public PenaltyStatus PenaltyStatus { get; set; }
    public DateTimeOffset Submitted { get; set; }
}
