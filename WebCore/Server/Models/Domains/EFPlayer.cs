using System.ComponentModel.DataAnnotations;
using BanHubData.Enums;

namespace BanHub.WebCore.Server.Models.Domains;

/// <summary>
/// Table for all players
/// </summary>
public class EFPlayer
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// The player's identity (Format: GAMEGUID:GAMEID)
    /// </summary>
    public required string Identity { get; init; }

    /// <summary>
    /// The last time the player was seen
    /// </summary>
    public required DateTimeOffset Heartbeat { get; set; }

    /// <summary>
    /// Total time played
    /// </summary>
    public required TimeSpan PlayTime { get; set; }

    /// <summary>
    /// Total connections to any server
    /// </summary>
    public required int TotalConnections { get; set; }

    /// <summary>
    /// The first time we saw them
    /// </summary>
    public required DateTimeOffset Created { get; init; }

    /// <summary>
    /// The player's web-login role
    /// </summary>
    public required WebRole WebRole { get; set; }

    /// <summary>
    /// The player's instance role
    /// </summary>
    public required CommunityRole CommunityRole { get; set; }

    /// <summary>
    /// Player's notes.
    /// </summary>
    public ICollection<EFNote> Notes { get; set; }

    /// <summary>
    /// The player's list of names and IP addresses
    /// </summary>
    public ICollection<EFAlias> Aliases { get; set; }

    /// <summary>
    /// The player's list of infractions
    /// </summary>
    public ICollection<EFPenalty> Penalties { get; set; }

    /// <summary>
    /// The lookup for current alias
    /// </summary>
    public EFCurrentAlias CurrentAlias { get; set; }

    /// <summary>
    /// The list of historically connected servers
    /// </summary>
    public ICollection<EFServerConnection> ServerConnections { get; set; }

    /// <summary>
    /// The player's chat messages
    /// </summary>
    public ICollection<EFChat> Chats { get; set; }
}
