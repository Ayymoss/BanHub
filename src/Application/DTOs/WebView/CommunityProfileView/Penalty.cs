using BanHub.Domain.Enums;

namespace BanHub.Application.DTOs.WebView.CommunityProfileView;

public class Penalty
{
    public Guid PenaltyGuid { get; set; }
    public string IssuerIdentity { get; set; }
    public string IssuerUserName { get; set; }
    public string RecipientIdentity { get; set; }
    public string RecipientUserName { get; set; }
    public string Reason { get; set; }
    public string? Evidence { get; set; }
    public Guid CommunityGuid { get; set; }
    public DateTimeOffset? Expiration { get; set; }
    public PenaltyType PenaltyType { get; set; }
    public PenaltyScope PenaltyScope { get; set; }
    public PenaltyStatus PenaltyStatus { get; set; }
    public DateTimeOffset Submitted { get; set; }
    public bool Automated { get; set; }
}
