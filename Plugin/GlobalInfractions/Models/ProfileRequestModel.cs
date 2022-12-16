
namespace GlobalInfractions.Models;

public class ProfileRequestModel
{
    /// <summary>
    /// The player's in-game GUID
    /// </summary>
    public string ProfileGuid { get; init; } = null!;

    /// <summary>
    /// The player's game tied GUID
    /// </summary>
    public string ProfileGame { get; init; } = null!;
    
    /// <summary>
    /// The player's reputation
    /// </summary>
    public int? Reputation { get; init; }
    
    /// <summary>
    /// The player's meta
    /// </summary>
    public virtual ProfileMetaRequestModel? ProfileMeta { get; set; }
    
    /// <summary>
    /// The player's list of infractions
    /// </summary>
    public virtual ICollection<InfractionRequestModel>? Infractions { get; set; } 
}

