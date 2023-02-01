using System.ComponentModel.DataAnnotations;

namespace BanHub.WebCore.Server.Models;

public class EFStatistic
{
    [Key] public int Id { get; set; }
    public required string Statistic { get; set; }
    public required int Count { get; set; }
}
