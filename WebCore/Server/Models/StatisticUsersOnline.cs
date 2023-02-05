namespace BanHub.WebCore.Server.Models;

public class StatisticUsersOnline
{
    public Guid InstanceGuid { get; set; }
    public int Online { get; set; }
    public DateTimeOffset HeartBeat { get; set; }
}
