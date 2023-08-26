using BanHub.WebCore.Server.Services;

namespace BanHub.WebCore.Server.Interfaces;

public interface ISentimentService
{
    float CalculateChatsSentiment(IEnumerable<SentimentService.Message> chats);
}
