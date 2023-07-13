
namespace BanHub.WebCore.Shared.Models.InstanceProfileView;

public class Instance
{
    public string? InstanceName { get; set; }
    public string InstanceIp { get; set; }
    public DateTimeOffset HeartBeat { get; set; }
    public bool Active { get; set; }
    public string? About { get; set; }
    public Dictionary<string, string>? Socials { get; set; }
    public DateTimeOffset Created { get; set; }
    public int ServerCount { get; set; }
    public Guid InstanceGuid { get; set; }
}
