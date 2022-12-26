using GlobalInfraction.WebCore.Shared.Enums;

namespace GlobalInfraction.WebCore.Shared.Models;

public class InfractionDto
{
    /// <summary>
    /// The type of infraction
    /// </summary>
    public InfractionType? InfractionType { get; set; }

    /// <summary>
    /// The state of the infraction
    /// </summary>
    public InfractionStatus? InfractionStatus { get; set; }

    /// <summary>
    /// The scope of the infraction
    /// </summary>
    public InfractionScope? InfractionScope { get; set; }

    /// <summary>
    /// The unique infraction identifier
    /// </summary>
    public Guid InfractionGuid { get; set; }

    /// <summary>
    /// Duration of a temporary infraction
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// The provided reason for the infraction
    /// </summary>
    public string? Reason { get; set; } 

    /// <summary>
    /// The uploaded evidence for the infraction
    /// </summary>
    public string? Evidence { get; set; }

    /// <summary>
    /// The admin GUID who issued the infraction
    /// </summary>
    public EntityDto? Admin { get; set; } 

    /// <summary>
    /// Information related to the player the infraction was issued to
    /// </summary>
    public EntityDto? Target { get; set; }

    /// <summary>
    /// Information related to the server the infraction was issued from
    /// </summary>
    public InstanceDto? Instance { get; set; } 
}
