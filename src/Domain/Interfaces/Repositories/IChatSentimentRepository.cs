using BanHub.Domain.Entities;

namespace BanHub.Domain.Interfaces.Repositories;

public interface IChatSentimentRepository
{
    Task<int?> AddOrUpdateSentimentAsync(EFChatSentiment sentiment, CancellationToken cancellationToken);
    Task<EFChatSentiment?> GetChatSentimentAsync(int playerId, CancellationToken cancellationToken);
}
