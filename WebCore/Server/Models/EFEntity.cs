using System.ComponentModel.DataAnnotations;
using BanHub.WebCore.Shared.Enums;

namespace BanHub.WebCore.Server.Models;

/// <summary>
/// Table for all players
/// </summary>
public class EFEntity
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// The player's identity (Format: GAMEGUID:GAMEID)
    /// </summary>
    public required string Identity { get; init; }

    /// <summary>
    /// The last time the player was seen
    /// </summary>
    public required DateTimeOffset HeartBeat { get; set; }

    /// <summary>
    /// The first time we saw them
    /// </summary>
    public required DateTimeOffset Created { get; init; }

    /// <summary>
    /// The player's web-login role
    /// </summary>
    public required WebRole WebRole { get; set; }

    /// <summary>
    /// Player's notes.
    /// </summary>
    public ICollection<EFNote> Notes { get; set; } = null!;

    /// <summary>
    /// The player's list of names and IP addresses
    /// </summary>
    public ICollection<EFAlias> Aliases { get; set; } = null!;

    /// <summary>
    /// The player's list of infractions
    /// </summary>
    public ICollection<EFPenalty> Penalties { get; set; } = null!;

    /// <summary>
    /// The lookup for current alias
    /// </summary>
    public EFCurrentAlias CurrentAlias { get; set; } = null!;

    /// <summary>
    /// The list of historically connected servers
    /// </summary>
    public ICollection<EFServerConnection> ServerConnections { get; set; } = null!;
    
    /// <summary>
    /// Penalty's identifiers
    /// </summary>
    public ICollection<EFPenaltyIdentifier> PenaltyIdentifiers { get; set; } = null!;
}
