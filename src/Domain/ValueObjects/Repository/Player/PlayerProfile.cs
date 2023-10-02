using BanHub.Domain.Enums;

namespace BanHub.Domain.ValueObjects.Repository.Player;

public class PlayerProfile
{
    public string Identity { get; set; }
    public string UserName { get; set; }
    public DateTimeOffset Heartbeat { get; set; }
    public int TotalConnections { get; set; }
    public TimeSpan PlayTime { get; set; }
    public DateTimeOffset Created { get; set; }
    public bool Connected { get; set; }
    public string? IpAddress { get; set; }
    public CommunityRole CommunityRole { get; set; }
    public WebRole WebRole { get; set; }
    public int PenaltyCount { get; set; }
    public int NoteCount { get; set; }
    public int ChatCount { get; set; }
}
