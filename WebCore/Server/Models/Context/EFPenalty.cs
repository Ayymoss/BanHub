using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Enums;

namespace BanHub.WebCore.Server.Models.Context;

/// <summary>
/// Table for all infractions 
/// </summary>
public class EFPenalty
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// The type of infraction
    /// </summary>
    public required PenaltyType PenaltyType { get; set; }

    /// <summary>
    /// The state of the infraction
    /// </summary>
    public required PenaltyStatus PenaltyStatus { get; set; }

    /// <summary>
    /// The scope of the infraction
    /// </summary>
    public required PenaltyScope PenaltyScope { get; set; }

    /// <summary>
    /// The unique infraction identifier
    /// </summary>
    public required Guid PenaltyGuid { get; set; }

    /// <summary>
    /// Time of the infraction
    /// </summary>
    public required DateTimeOffset Submitted { get; set; }

    /// <summary>
    /// Duration of a temporary infraction
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// The provided reason for the infraction
    /// </summary>
    public required string Reason { get; set; }

    /// <summary>
    /// Automated penalty
    /// </summary>
    public required bool Automated { get; set; }

    /// <summary>
    /// The uploaded evidence for the infraction
    /// </summary>
    public string? Evidence { get; set; }

    /// <summary>
    /// Penalty's identifiers
    /// </summary>
    public EFPenaltyIdentifier Identifier { get; set; } = null!;

    /// <summary>
    /// The admin GUID who issued the infraction
    /// </summary>
    public int AdminId { get; set; }

    [ForeignKey(nameof(AdminId))] public EFPlayer Admin { get; set; } = null!;

    /// <summary>
    /// The user GUID who received the infraction
    /// </summary>
    public int TargetId { get; set; }

    [ForeignKey(nameof(TargetId))] public EFPlayer Target { get; set; } = null!;

    /// <summary>
    /// The server reference this infraction
    /// </summary>
    public int InstanceId { get; set; }

    [ForeignKey(nameof(InstanceId))] public EFInstance Instance { get; set; } = null!;
}
