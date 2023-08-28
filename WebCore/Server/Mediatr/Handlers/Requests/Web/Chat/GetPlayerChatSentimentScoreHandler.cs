using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Models.Domains;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Mediatr.Commands.Players;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Web.Chat;

public class GetPlayerChatSentimentScoreHandler : IRequestHandler<GetPlayerChatSentimentScoreCommand, float?>
{
    private readonly DataContext _context;
    private readonly ISentimentService _sentimentService;

    public GetPlayerChatSentimentScoreHandler(DataContext context, ISentimentService sentimentService)
    {
        _context = context;
        _sentimentService = sentimentService;
    }

    public async Task<float?> Handle(GetPlayerChatSentimentScoreCommand request, CancellationToken cancellationToken)
    {
        var player = await _context.Players
            .Where(x => x.Identity == request.Identity)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (player is null) return null;

        var currentSentiment = await _context.ChatSentiments
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
            _context.ChatSentiments.Add(currentSentiment);
        }

        var newPlayerChats = await _context.Chats
            .Where(x => x.Player.Identity == request.Identity)
            .Where(x => x.Submitted > currentSentiment.LastChatCalculated)
            .ToListAsync(cancellationToken: cancellationToken);

        if (newPlayerChats.Count < 10) return currentSentiment.Sentiment;

        var newSentiment = _sentimentService.CalculateChatsSentiment(newPlayerChats.Select(x => new SentimentService.Message(x.Message)));

        currentSentiment.Sentiment = (currentSentiment.Sentiment * currentSentiment.ChatCount + newSentiment * newPlayerChats.Count) /
                                     (currentSentiment.ChatCount + newPlayerChats.Count);

        currentSentiment.ChatCount += newPlayerChats.Count;
        currentSentiment.LastChatCalculated = newPlayerChats.Max(x => x.Submitted);

        await _context.SaveChangesAsync(cancellationToken);
        return currentSentiment.Sentiment;
    }
}
