using BanHubData.Enums;

namespace BanHub.WebCore.Shared.Models.PlayerProfileView;

public class Player
{
    public string Identity { get; set; }
    public string UserName { get; set; }
    public DateTimeOffset HeartBeat { get; set; }
    public bool IsGloballyBanned { get; set; }
    public int TotalConnections { get; set; }
    public TimeSpan PlayTime { get; set; }
    public DateTimeOffset Created { get; set; }
    public bool Connected { get; set; }
    public string? IpAddress { get; set; }
    public CommunityRole CommunityRole { get; set; }
    public WebRole WebRole { get; set; }
    public string? LastConnectedServerName { get; set; }
    public string? LastConnectedCommunityName { get; set; }
    public int PenaltyCount { get; set; }
    public int NoteCount { get; set; }
    public int ChatCount { get; set; }
    public float? ChatSentimentScore { get; set; }
}
