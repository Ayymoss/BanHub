namespace BanHub.WebCore.Shared.Models.InstancesView;

public class Instance
{
    public bool Active { get; set; }
    public Guid InstanceGuid { get; set; }
    public string InstanceName { get; set; }
    public string InstanceIp { get; set; }
    public int ServerCount { get; set; }
    public DateTimeOffset HeartBeat { get; set; }
    public DateTimeOffset Created { get; set; }
}
