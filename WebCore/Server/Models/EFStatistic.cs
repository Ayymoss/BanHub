using System.ComponentModel.DataAnnotations;

namespace GlobalInfraction.WebCore.Server.Models;

public class EFStatistic
{
    [Key] public int Id { get; set; }
    public string Statistic { get; set; } = null!;
    public int Count { get; set; }
}
