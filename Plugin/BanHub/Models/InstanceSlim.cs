namespace BanHub.Models;

public class InstanceSlim
{
    public Guid InstanceGuid { get; set; }
    public string InstanceIp { get; set; }
    public Guid ApiKey { get; set; }
    public bool Active { get; set; }
}
