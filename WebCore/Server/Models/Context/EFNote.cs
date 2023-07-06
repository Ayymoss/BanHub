using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHub.WebCore.Server.Models.Context;

public class EFNote
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// Identifier for note
    /// </summary>
    public required Guid NoteGuid { get; set; }

    /// <summary>
    /// Time the note was issued
    /// </summary>
    public required DateTimeOffset Created { get; set; }

    /// <summary>
    /// The associated note
    /// </summary>
    public required string Message { get; set; }
    
    /// <summary>
    /// Is the note public
    /// </summary>
    public required bool IsPrivate { get; set; }

    /// <summary>
    /// The admin who issued the note
    /// </summary>
    public int AdminId { get; set; }

    [ForeignKey(nameof(AdminId))] public EFPlayer Admin { get; set; } = null!;

    /// <summary>
    /// The user who received the note
    /// </summary>
    public int TargetId { get; set; }

    [ForeignKey(nameof(TargetId))] public EFPlayer Target { get; set; } = null!;
}
