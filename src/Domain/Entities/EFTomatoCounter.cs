using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHub.Domain.Entities;

public class EFTomatoCounter
{
    [Key] public int Id { get; set; }

    public int Tomatoes { get; set; }

    public int PlayerId { get; set; }

    [ForeignKey(nameof(PlayerId))] public EFPlayer Player { get; set; }
}
