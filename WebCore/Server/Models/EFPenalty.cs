using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BanHub.WebCore.Shared.Enums;

namespace BanHub.WebCore.Server.Models;

/// <summary>
/// Table for all infractions 
/// </summary>
public class EFPenalty
{
    [Key] public int Id { get; set; }

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
    public PenaltyScope PenaltyScope { get; set; }

    /// <summary>
    /// The unique infraction identifier
    /// </summary>
    public Guid PenaltyGuid { get; set; }

    /// <summary>
    /// Time of the infraction
    /// </summary>
    public DateTimeOffset Submitted { get; set; }

    /// <summary>
    /// Duration of a temporary infraction
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// The provided reason for the infraction
    /// </summary>
    public string Reason { get; set; } = null!;

    /// <summary>
    /// The uploaded evidence for the infraction
    /// </summary>
    public string? Evidence { get; set; }

    /// <summary>
    /// The admin GUID who issued the infraction
    /// </summary>
    public int AdminId { get; set; }

    [ForeignKey(nameof(AdminId))] public virtual EFEntity Admin { get; set; } = null!;

    /// <summary>
    /// The user GUID who received the infraction
    /// </summary>
    public int TargetId { get; set; }

    [ForeignKey(nameof(TargetId))] public virtual EFEntity Target { get; set; } = null!;

    /// <summary>
    /// The server reference this infraction
    /// </summary>
    public int InstanceId { get; set; }

    [ForeignKey(nameof(InstanceId))] public virtual EFInstance Instance { get; set; } = null!;
}
