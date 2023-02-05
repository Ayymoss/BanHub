using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHub.WebCore.Server.Models.Context;

public class EFNote
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// Time the note was issued
    /// </summary>
    public required DateTimeOffset Created { get; set; }

    /// <summary>
    /// The associated note
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// The admin who issued the note
    /// </summary>
    public int AdminId { get; set; }

    [ForeignKey(nameof(AdminId))] public EFEntity Admin { get; set; } = null!;

    /// <summary>
    /// The user who received the note
    /// </summary>
    public int TargetId { get; set; }

    [ForeignKey(nameof(TargetId))] public EFEntity Target { get; set; } = null!;
}
