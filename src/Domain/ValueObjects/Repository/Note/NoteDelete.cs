namespace BanHub.Domain.ValueObjects.Repository.Note;

public class NoteDelete
{
    public Guid NoteGuid { get; set; }
    public string IssuerUserName { get; set; }
    public string IssuerIdentity { get; set; }
    public string RecipientUserName { get; set; }
    public string RecipientIdentity { get; set; }
    public string Message { get; set; }
    public bool IsPrivate { get; set; }
}
