namespace BanHub.Domain.ValueObjects.Plugin;

public class AddPlayerPenaltyEvidenceCommandSlim
{
    public Guid PenaltyGuid { get; set; }
    public string Evidence { get; set; }
    public string? IssuerIdentity { get; set; }
    public string? IssuerUsername { get; set; }
}
