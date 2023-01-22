using GlobalInfractions.Enums;

namespace GlobalInfractions.Models;

public class EntityDto
{
    /// <summary>
    /// The player's identity
    /// </summary>
    public string Identity { get; set; } = null!;

    /// <summary>
    /// The player's strike count
    /// </summary>
    public int? Strike { get; set; }

    /// <summary>
    /// The last time the player was seen
    /// </summary>
    public DateTimeOffset? HeartBeat { get; set; }
    
    /// <summary>
    /// The first time we saw the player
    /// </summary>
    public DateTimeOffset? Created { get; set; }
    
    /// <summary>
    /// The player's web-login role
    /// </summary>
    public WebRole WebRole { get; set; }

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
    
    /// <summary>
    /// Servers the client has connected to
    /// </summary>
    public virtual ICollection<ServerDto>? Servers { get; set; }
    
    /// <summary>
    /// Player's notes.
    /// </summary>
    public virtual ICollection<NoteDto>? Notes { get; set; }
    
    /// <summary>
    /// Server the client is connected to
    /// </summary>
    public virtual ServerDto? Server { get; set; }
}
