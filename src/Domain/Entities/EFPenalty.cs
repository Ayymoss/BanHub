using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BanHub.Domain.Enums;

namespace BanHub.Domain.Entities;

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
    /// Expiration of a temporary infraction
    /// </summary>
    public DateTimeOffset? Expiration { get; set; }

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
    public EFPenaltyIdentifier? Identifier { get; set; }

    /// <summary>
    /// The admin GUID who issued the infraction
    /// </summary>
    public int IssuerId { get; set; }

    [ForeignKey(nameof(IssuerId))] public EFPlayer Issuer { get; set; }

    /// <summary>
    /// The user GUID who received the infraction
    /// </summary>
    public int RecipientId { get; set; }

    [ForeignKey(nameof(RecipientId))] public EFPlayer Recipient { get; set; }

    /// <summary>
    /// The server reference this infraction
    /// </summary>
    public int CommunityId { get; set; }

    [ForeignKey(nameof(CommunityId))] public EFCommunity Community { get; set; }
}
