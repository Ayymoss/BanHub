using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHub.WebCore.Server.Models.Domains;

public class EFTomatoCounter
{
    [Key] public int Id { get; set; }

    public int Tomatoes { get; set; }

    public int PlayerId { get; set; }

    [ForeignKey(nameof(PlayerId))] public EFPlayer Player { get; set; }
}
