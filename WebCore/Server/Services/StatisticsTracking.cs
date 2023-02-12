using BanHub.WebCore.Server.Models;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Services;

public class StatisticsTracking
{
    public int Penalties;
    public int Servers;
    public int Instances;
    public int Entities;
    public List<StatisticBan> BansDay { get; set; } = new();
    public int BansDayCount { get; set; }
    public List<StatisticUsersOnline> UsersOnline { get; set; } = new();
    public int UsersOnlineCount { get; set; }
    public bool Loaded { get; set; }
}
