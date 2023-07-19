
namespace BanHub.WebCore.Shared.Models.InstanceProfileView;

public class Instance
{
    public Guid InstanceGuid { get; set; }
    public string InstanceName { get; set; }
    public string InstanceWebsite { get; set; }
    public bool Connected { get; set; }
    public bool Active { get; set; }
    public string? About { get; set; }
    public Dictionary<string, string>? Socials { get; set; }
    public DateTimeOffset Created { get; set; }
    public int ServerCount { get; set; }
    public int InstancePort { get; set; }
}
