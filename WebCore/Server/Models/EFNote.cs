using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalInfraction.WebCore.Server.Models;

public class EFNote
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// Time the note was issued
    /// </summary>
    public DateTimeOffset Created { get; init; }

    /// <summary>
    /// The associated note
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// The admin who issued the note
    /// </summary>
    public int AdminId { get; set; }

    [ForeignKey(nameof(AdminId))] public virtual EFEntity Admin { get; set; } = null!;

    /// <summary>
    /// The user who received the note
    /// </summary>
    public int TargetId { get; set; }

    [ForeignKey(nameof(TargetId))] public virtual EFEntity Target { get; set; } = null!;
}
