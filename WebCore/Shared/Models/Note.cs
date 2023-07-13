namespace BanHubData.Domains;

public class Note
{
    /// <summary>
    /// The database guid for the note
    /// </summary>
    public Guid? NoteGuid { get; set; }
    
    /// <summary>
    /// Time the note was issued
    /// </summary>
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// The associated note
    /// </summary>
    public string Message { get; set; }
    
    /// <summary>
    /// Is the note public
    /// </summary>
    public bool IsPrivate { get; set; }
    
    /// <summary>
    /// Reason for the deletion (Web)
    /// </summary>
    public string DeletionReason { get; set; }



    
}
