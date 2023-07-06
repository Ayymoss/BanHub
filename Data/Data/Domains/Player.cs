using Data.Enums;

namespace Data.Domains;

public class Player
{
    /// <summary>
    /// The player's identity
    /// </summary>
    public string Identity { get; set; } = null!;

    /// <summary>
    /// The last time the player was seen
    /// </summary>
    public DateTimeOffset HeartBeat { get; set; }

    /// <summary>
    /// The first time we saw the player
    /// </summary>
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// Total time played
    /// </summary>
    public TimeSpan PlayTime { get; set; }

    /// <summary>
    /// Total connections to any server
    /// </summary>
    public int TotalConnections { get; set; }

    /// <summary>
    /// Is the player globally banned via IP or GUID
    /// </summary>
    public bool HasIdentityBan { get; set; }

    /// <summary>
    /// The player's instance role
    /// </summary>
    public InstanceRole InstanceRole { get; set; }

    /// <summary>
    /// The player's meta
    /// </summary>
    public virtual Alias Alias { get; set; }

    /// <summary>
    /// Player's notes.
    /// </summary>
    public virtual ICollection<Note> Notes { get; set; }

    /// <summary>
    /// The associated instance
    /// </summary>
    public virtual Instance Instance { get; set; }

    /// <summary>
    /// The player's list of infractions
    /// </summary>
    public virtual ICollection<Penalty> Penalties { get; set; }

    /// <summary>
    /// Servers the client has connected to
    /// </summary>
    public virtual ICollection<Server>? Servers { get; set; }

    /// <summary>
    /// Server the client is connected to
    /// </summary>
    public virtual Server Server { get; set; }
}
