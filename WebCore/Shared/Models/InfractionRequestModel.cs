using GlobalInfraction.WebCore.Shared.Enums;

namespace GlobalInfraction.WebCore.Shared.Models;

public class InfractionRequestModel
{
    /// <summary>
    /// The type of infraction
    /// </summary>
    public InfractionType InfractionType { get; set; }

    /// <summary>
    /// The scope of the infraction
    /// </summary>
    public InfractionScope InfractionScope { get; set; }
    
    /// <summary>
    /// The unique infraction identifier
    /// </summary>
    public Guid InfractionGuid { get; set; }

    /// <summary>
    /// The admin GUID who issued the infraction
    /// </summary>
    public string AdminGuid { get; set; } = null!;

    /// <summary>
    ///  The admin who issued the infraction
    /// </summary>
    public string AdminUserName { get; set; } = null!;

    /// <summary>
    /// The provided reason for the infraction
    /// </summary>
    public string Reason { get; set; } = null!;

    /// <summary>
    /// The uploaded evidence for the infraction
    /// </summary>
    public string? Evidence { get; set; }

    /// <summary>
    /// Information related to the server the infraction was issued from
    /// </summary>
    public InstanceRequestModel Instance { get; set; } = null!;

    /// <summary>
    /// Information related to the player the infraction was issued to
    /// </summary>
    public ProfileRequestModel Profile { get; set; } = null!;
}
