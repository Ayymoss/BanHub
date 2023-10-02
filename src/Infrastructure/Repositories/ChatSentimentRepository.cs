using BanHub.Domain.Entities;
using BanHub.Domain.Interfaces.Repositories;
using BanHub.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BanHub.Infrastructure.Repositories;

public class ChatSentimentRepository(DataContext context) : IChatSentimentRepository
{
    public async Task<EFChatSentiment?> GetChatSentimentAsync(int playerId, CancellationToken cancellationToken)
    {
        var currentSentiment = await context.ChatSentiments
            .Where(x => x.Player.Id == playerId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return currentSentiment;
    }

    public async Task<int?> AddOrUpdateSentimentAsync(EFChatSentiment sentiment, CancellationToken cancellationToken)
    {
        var chatSentiment = await context.ChatSentiments
            .Where(x => x.Id == sentiment.Id)
            .AnyAsync(cancellationToken: cancellationToken);

        if (chatSentiment) context.ChatSentiments.Update(sentiment);
        else context.ChatSentiments.Add(sentiment);

        await context.SaveChangesAsync(cancellationToken);
        return sentiment.Id;
    }
}
