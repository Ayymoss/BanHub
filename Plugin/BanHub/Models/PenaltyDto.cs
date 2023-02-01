using BanHub.Enums;

namespace BanHub.Models;

public class PenaltyDto
{
    /// <summary>
    /// The type of infraction
    /// </summary>
    public PenaltyType? PenaltyType { get; set; }

    /// <summary>
    /// The state of the infraction
    /// </summary>
    public PenaltyStatus? PenaltyStatus { get; set; }

    /// <summary>
    /// The scope of the infraction
    /// </summary>
    public PenaltyScope? PenaltyScope { get; set; }

    /// <summary>
    /// The unique infraction identifier
    /// </summary>
    public Guid PenaltyGuid { get; set; }

    /// <summary>
    /// Duration of a temporary infraction
    /// </summary>
    public TimeSpan? Duration { get; set; }
    
    /// <summary>
    /// The date of the infraction
    /// </summary>
    public DateTimeOffset? Submitted { get; set; }
    
    /// <summary>
    /// AntiCheat Reason
    /// </summary>
    public string? AntiCheatReason { get; set; }
    
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

