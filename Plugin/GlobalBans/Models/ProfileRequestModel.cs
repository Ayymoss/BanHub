
namespace GlobalBans.Models;

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
    /// Unique profile identifier for comparing profiles
    /// </summary>
    public string ProfileIdentifier { get; init; } = null!;

    /// <summary>
    /// The player's reputation
    /// </summary>
    public int Reputation { get; }

    /// <summary>
    /// The player's list of names and IP addresses
    /// </summary>
    public virtual ProfileMetaRequestModel ProfileMeta { get; set; } = null!;
}
