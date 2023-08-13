namespace BanHub.WebCore.Shared.Models.Shared;

public class Statistic
{
    /// <summary>
    /// The count of how many entities there are
    /// </summary>
    public int PlayerCount { get; set; }

    /// <summary>
    /// The count of how many instances there are
    /// </summary>
    public int CommunityCount { get; set; }

    /// <summary>
    /// The count of how many servers there are
    /// </summary>
    public int ServerCount { get; set; }

    /// <summary>
    /// The count of how many infractions there are
    /// </summary>
    public int PenaltyCount { get; set; }
}
