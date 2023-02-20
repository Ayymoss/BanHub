namespace BanHub.WebCore.Shared.DTOs;

public class NoteDto
{
    /// <summary>
    /// The database guid for the note
    /// </summary>
    public Guid? NoteGuid { get; set; }
    
    /// <summary>
    /// Time the note was issued
    /// </summary>
    public DateTimeOffset? Created { get; set; }

    /// <summary>
    /// The associated note
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// Is the note public
    /// </summary>
    public bool? IsPrivate { get; set; }
    
    /// <summary>
    /// Reason for the deletion (Web)
    /// </summary>
    public string? DeletionReason { get; set; }

    /// <summary>
    /// The admin who issued the note
    /// </summary>
    public virtual EntityDto? Admin { get; set; }

    /// <summary>
    /// The user who received the note
    /// </summary>
    public virtual EntityDto? Target { get; set; }

    
}
