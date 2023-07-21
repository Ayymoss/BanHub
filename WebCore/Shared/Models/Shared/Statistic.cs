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

    /// <summary>
    /// The count of how many users are online
    /// </summary>
    public int OnlinePlayersCount { get; set; }

    /// <summary>
    /// The count of how many bans there are in last 7d
    /// </summary>
    public int RecentBansCount { get; set; }
}
