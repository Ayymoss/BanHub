using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Domains;
using BanHub.WebCore.Server.Mediatr.Commands.Requests;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.Players;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Web.Chat;

public class GetPlayerChatSentimentScoreHandler(DataContext context, ISender sender) 
    : IRequestHandler<GetPlayerChatSentimentScoreCommand, float?>
{
    public async Task<float?> Handle(GetPlayerChatSentimentScoreCommand request, CancellationToken cancellationToken)
    {
        var player = await context.Players
            .Where(x => x.Identity == request.Identity)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (player is null) return null;

        var currentSentiment = await context.ChatSentiments
            .Where(x => x.Player.Id == player.Id)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (currentSentiment is null)
        {
            currentSentiment = new EFChatSentiment
            {
                Sentiment = 0,
                ChatCount = 0,
                LastChatCalculated = default,
                PlayerId = player.Id,
            };
            context.ChatSentiments.Add(currentSentiment);
        }

        var newPlayerChats = await context.Chats
            .Where(x => x.Player.Identity == request.Identity)
            .Where(x => x.Submitted > currentSentiment.LastChatCalculated)
            .ToListAsync(cancellationToken: cancellationToken);

        if (newPlayerChats.Count < 10) return currentSentiment.Sentiment;

        var newSentiment = await sender.Send(new CalculateChatSentimentCommand
            {Messages = newPlayerChats.Select(x => new Message(x.Message))}, cancellationToken);

        currentSentiment.Sentiment = (currentSentiment.Sentiment * currentSentiment.ChatCount + newSentiment * newPlayerChats.Count) /
                                     (currentSentiment.ChatCount + newPlayerChats.Count);

        currentSentiment.ChatCount += newPlayerChats.Count;
        currentSentiment.LastChatCalculated = newPlayerChats.Max(x => x.Submitted);

        await context.SaveChangesAsync(cancellationToken);
        return currentSentiment.Sentiment;
    }
}
