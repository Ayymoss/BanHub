using System.Text.Json.Serialization;
using Data.Enums;

namespace Data.Domains;

public class Penalty
{
    /// <summary>
    /// The type of infraction
    /// </summary>
    public PenaltyType PenaltyType { get; set; }

    /// <summary>
    /// The state of the infraction
    /// </summary>
    public PenaltyStatus PenaltyStatus { get; set; }

    /// <summary>
    /// The scope of the infraction
    /// </summary>
    public PenaltyScope? PenaltyScope { get; set; }

    /// <summary>
    /// The unique infraction identifier
    /// </summary>
    public Guid PenaltyGuid { get; set; }

    /// <summary>
    /// Duration of a temporary infraction
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// The date of the infraction
    /// </summary>
    public DateTimeOffset Submitted { get; set; }

    /// <summary>
    /// The provided reason for the infraction
    /// </summary>
    public string Reason { get; set; }

    /// <summary>
    /// Was the ban automated?
    /// </summary>
    public bool Automated { get; set; }

    /// <summary>
    /// The uploaded evidence for the infraction
    /// </summary>
    public string? Evidence { get; set; }

    /// <summary>
    /// For privileged users, the reason for the infraction deletion
    /// </summary>
    public string DeletionReason { get; set; }

    /// <summary>
    /// The admin GUID who issued the infraction
    /// </summary>
    public string AdminIdentity { get; set; }

    /// <summary>
    /// Information related to the player the infraction was issued to
    /// </summary>
    public string TargetIdentity { get; set; }

    /// <summary>
    /// Information related to the server the infraction was issued from
    /// </summary>
    public Guid InstanceGuid { get; set; }
}
