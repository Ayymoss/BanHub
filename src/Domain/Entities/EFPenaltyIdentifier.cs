﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHub.Domain.Entities;

public class EFPenaltyIdentifier
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// Client's Identity
    /// </summary>
    public required string Identity { get; set; }

    /// <summary>
    /// Client's IpAddress
    /// </summary>
    public required string IpAddress { get; set; }

    /// <summary>
    /// Expiration of penalty
    /// </summary>
    public required DateTimeOffset Expiration { get; set; }

    /// <summary>
    /// Referenced penalty
    /// </summary>
    public int PenaltyId { get; set; }

    [ForeignKey(nameof(PenaltyId))] public EFPenalty Penalty { get; set; }
}
