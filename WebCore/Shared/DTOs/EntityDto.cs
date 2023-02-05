using BanHub.WebCore.Shared.Enums;

namespace BanHub.WebCore.Shared.DTOs;

public class EntityDto
{
    /// <summary>
    /// The synchronised API version number.
    /// </summary>
    public int ApiVersion { get; set; } = 1;

    /// <summary>
    /// The player's identity
    /// </summary>
    public string Identity { get; set; } = null!;

    /// <summary>
    /// The last time the player was seen
    /// </summary>
    public DateTimeOffset? HeartBeat { get; set; }

    /// <summary>
    /// The first time we saw the player
    /// </summary>
    public DateTimeOffset? Created { get; set; }

    /// <summary>
    /// Is the player globally banned via IP or GUID
    /// </summary>
    public bool HasIdentityBan { get; set; }

    /// <summary>
    /// The player's instance role
    /// </summary>
    public InstanceRole? InstanceRole { get; set; }

    /// <summary>
    /// The player's meta
    /// </summary>
    public virtual AliasDto? Alias { get; set; }

    /// <summary>
    /// Player's notes.
    /// </summary>
    public virtual ICollection<NoteDto>? Notes { get; set; }

    /// <summary>
    /// The associated instance
    /// </summary>
    public virtual InstanceDto? Instance { get; set; }

    /// <summary>
    /// The player's list of infractions
    /// </summary>
    public virtual ICollection<PenaltyDto>? Penalties { get; set; }

    /// <summary>
    /// Servers the client has connected to
    /// </summary>
    public virtual ICollection<ServerDto>? Servers { get; set; }

    /// <summary>
    /// Server the client is connected to
    /// </summary>
    public virtual ServerDto? Server { get; set; }
}
