using BanHub.WebCore.Server.Models;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Services;

public class StatisticsTracking
{
    public int Penalties;
    public int Servers;
    public int Instances;
    public int Entities;
    public ICollection<StatisticBan> BansDay { get; set; } = null!;
    public int BansDayCount { get; set; }
    public ICollection<StatisticUsersOnline> UsersOnline { get; set; } = null!;
    public int UsersOnlineCount { get; set; }
    public bool Loaded { get; set; }
}
