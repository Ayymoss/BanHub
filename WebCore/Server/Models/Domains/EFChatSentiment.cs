using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHub.WebCore.Server.Models.Domains;

public class EFChatSentiment
{
    [Key] public int Id { get; set; }
    public float Sentiment { get; set; }
    public int ChatCount { get; set; }
    public DateTimeOffset LastChatCalculated { get; set; }

    /// <summary>
    /// The user whose sentiment we are tracking
    /// </summary>
    public int PlayerId { get; set; }

    [ForeignKey(nameof(PlayerId))] public EFPlayer Player { get; set; }
}
