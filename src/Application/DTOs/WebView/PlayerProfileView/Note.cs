namespace BanHub.Application.DTOs.WebView.PlayerProfileView;

public class Note
{
    public Guid NoteGuid { get; set; }
    public string Message { get; set; }
    public string IssuerUserName { get; set; }
    public DateTimeOffset Created { get; set; }
    public bool IsPrivate { get; set; }
    public string AdminIdentity { get; set; }
}
