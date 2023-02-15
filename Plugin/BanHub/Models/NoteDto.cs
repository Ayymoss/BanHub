namespace BanHub.Models;

public class NoteDto
{
    /// <summary>
    /// Time the note was issued
    /// </summary>
    public DateTimeOffset? Created { get; init; }

    /// <summary>
    /// The associated note
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// The admin who issued the note
    /// </summary>
    public virtual EntityDto? Admin { get; set; }

    /// <summary>
    /// Is the note public
    /// </summary>
    public bool? IsPrivate { get; set; }

    /// <summary>
    /// The user who received the note
    /// </summary>
    public virtual EntityDto? Target { get; set; }
}
