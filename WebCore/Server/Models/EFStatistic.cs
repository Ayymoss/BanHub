using System.ComponentModel.DataAnnotations;

namespace BanHub.WebCore.Server.Models;

public class EFStatistic
{
    [Key] public int Id { get; set; }
    public string Statistic { get; set; } = null!;
    public int Count { get; set; }
}
