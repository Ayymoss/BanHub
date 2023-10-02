namespace BanHub.Domain.ValueObjects.Repository.Chat;

public class ChatContext
{
    public DateTimeOffset Submitted { get; set; }
    public string PlayerUserName { get; set; }
    public string PlayerIdentity { get; set; }
    public string Message { get; set; }
}
