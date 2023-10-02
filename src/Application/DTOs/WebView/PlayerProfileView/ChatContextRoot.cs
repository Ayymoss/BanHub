using BanHub.Domain.ValueObjects.Repository.Chat;

namespace BanHub.Application.DTOs.WebView.PlayerProfileView;

public class ChatContextRoot
{
    public bool Loaded { get; set; }
    public IEnumerable<ChatContext> Messages { get; set; }
}

