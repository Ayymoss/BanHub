using System.ComponentModel.DataAnnotations;
using GlobalInfraction.WebCore.Shared.Enums;

namespace GlobalInfraction.WebCore.Server.Models;

/// <summary>
/// Table for all players
/// </summary>
public class EFEntity
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// The player's identity (Format: GAMEGUID:GAMEID)
    /// </summary>
    public string Identity { get; init; } = null!;

    /// <summary>
    /// The player's strike count
    /// </summary>
    public int Strike { get; set; }

    /// <summary>
    /// The last time the player was seen
    /// </summary>
    public DateTimeOffset HeartBeat { get; set; }

    /// <summary>
    /// The first time we saw them
    /// </summary>
    public DateTimeOffset Created { get; init; }

    /// <summary>
    /// The player's web-login role
    /// </summary>
    public WebRole WebRole { get; set; }

    /// <summary>
    /// Player's notes.
    /// </summary>
    public virtual ICollection<EFNote> Notes { get; set; } = null!;

    /// <summary>
    /// The player's list of names and IP addresses
    /// </summary>
    public virtual ICollection<EFAlias> Aliases { get; set; } = null!;

    /// <summary>
    /// The player's list of infractions
    /// </summary>
    public virtual ICollection<EFInfraction> Infractions { get; set; } = null!;

    /// <summary>
    /// The lookup for current alias
    /// </summary>
    public virtual EFCurrentAlias CurrentAlias { get; set; } = null!;

    /// <summary>
    /// The list of historically connected servers
    /// </summary>
    public virtual ICollection<EFServerConnection> ServerConnections { get; set; } = null!;
}
