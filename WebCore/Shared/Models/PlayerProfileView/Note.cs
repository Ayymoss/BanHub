namespace BanHub.WebCore.Shared.Models.PlayerProfileView;

public class Note
{
    public Guid NoteGuid { get; set; }
    public string Message { get; set; }
    public string AdminUserName { get; set; }
    public DateTimeOffset Created { get; set; }
    public bool IsPrivate { get; set; }
}
