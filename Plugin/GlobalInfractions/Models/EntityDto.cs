namespace GlobalInfractions.Models;

public class EntityDto
{
    /// <summary>
    /// The player's identity
    /// </summary>
    public string Identity { get; init; } = null!;

    /// <summary>
    /// The player's reputation
    /// </summary>
    public int? Reputation { get; init; }

    /// <summary>
    /// The last time the player was seen
    /// </summary>
    public DateTimeOffset? HeartBeat { get; set; }

    /// <summary>
    /// The player's meta
    /// </summary>
    public virtual AliasDto? Alias { get; set; }

    /// <summary>
    /// The associated instance
    /// </summary>
    public virtual InstanceDto? Instance { get; set; }

    /// <summary>
    /// The player's list of infractions
    /// </summary>
    public virtual ICollection<InfractionDto>? Infractions { get; set; }
}
