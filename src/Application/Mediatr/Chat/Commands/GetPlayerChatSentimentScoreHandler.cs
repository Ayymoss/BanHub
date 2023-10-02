using BanHub.Application.Mediatr.Player.Commands;
using BanHub.Application.Mediatr.Services.Commands;
using BanHub.Domain.Entities;
using BanHub.Domain.Interfaces.Repositories;
using MediatR;

namespace BanHub.Application.Mediatr.Chat.Commands;

public class GetPlayerChatSentimentScoreHandler(ISender sender, IChatSentimentRepository chatSentimentRepository,
    IPlayerRepository playerRepository, IChatRepository chatRepository) : IRequestHandler<GetPlayerChatSentimentScoreCommand, float?>
{
    public async Task<float?> Handle(GetPlayerChatSentimentScoreCommand request, CancellationToken cancellationToken)
    {
        var player = await playerRepository.GetPlayerByIdentityAsync(request.Identity, cancellationToken);
        if (player is null) return null;

        var currentSentiment = await chatSentimentRepository.GetChatSentimentAsync(player.Id, cancellationToken);
        if (currentSentiment is null)
        {
            currentSentiment = new EFChatSentiment
            {
                Sentiment = 0,
                ChatCount = 0,
                LastChatCalculated = default,
                PlayerId = player.Id
            };
            await chatSentimentRepository.AddOrUpdateSentimentAsync(currentSentiment, cancellationToken);
        }

        var newPlayerChats =
            await chatRepository.GetNewPlayerChatsAsync(request.Identity, currentSentiment.LastChatCalculated, cancellationToken);

        var playerChats = newPlayerChats as EFChat[] ?? newPlayerChats.ToArray();
        if (playerChats.Length < 10) return currentSentiment.Sentiment;

        var newSentiment = await sender.Send(new CalculateChatSentimentCommand
            {Messages = playerChats.Select(x => new Message(x.Message))}, cancellationToken);

        currentSentiment.Sentiment = (currentSentiment.Sentiment * currentSentiment.ChatCount + newSentiment * playerChats.Length) /
                                     (currentSentiment.ChatCount + playerChats.Length);

        currentSentiment.ChatCount += playerChats.Length;
        currentSentiment.LastChatCalculated = playerChats.Max(x => x.Submitted);
        await chatSentimentRepository.AddOrUpdateSentimentAsync(currentSentiment, cancellationToken);
        return currentSentiment.Sentiment;
    }
}
