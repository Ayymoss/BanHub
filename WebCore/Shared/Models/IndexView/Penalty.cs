namespace BanHub.WebCore.Shared.Models.IndexView;

public class Penalty
{
    public string InstanceName { get; set; }
    public Guid InstanceGuid { get; set; }
    public string TargetUserName { get; set; }
    public string TargetIdentity { get; set; }
    public string Reason { get; set; }
    public DateTimeOffset Submitted { get; set; }
}
