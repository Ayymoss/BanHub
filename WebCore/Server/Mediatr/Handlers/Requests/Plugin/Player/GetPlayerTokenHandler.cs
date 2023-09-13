using System.Text;
using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Domains;
using BanHubData.Mediatr.Commands.Requests.Player;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Plugin.Player;

public class GetPlayerTokenHandler : IRequestHandler<GetPlayerTokenCommand, string?>
{
    private readonly DataContext _context;

    public GetPlayerTokenHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<string?> Handle(GetPlayerTokenCommand request, CancellationToken cancellationToken)
    {
        var player = await _context.Players.FirstOrDefaultAsync(x => x.Identity == request.Identity, cancellationToken: cancellationToken);
        if (player is null) return null;

        var utcTimeNow = DateTimeOffset.UtcNow;

        var hasActiveToken = await _context.AuthTokens.FirstOrDefaultAsync(x =>
            x.PlayerId == player.Id && x.Expiration > utcTimeNow && !x.Used, cancellationToken: cancellationToken);
        if (hasActiveToken is not null) return hasActiveToken.Token;

        const string characters = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ23456789";
        var result = new StringBuilder(6);
        for (var i = 0; i < 6; i++) result.Append(characters[Random.Shared.Next(characters.Length)]);

        var token = new EFAuthToken
        {
            Token = result.ToString(),
            Expiration = utcTimeNow + TimeSpan.FromMinutes(5),
            PlayerId = player.Id,
            Used = false
        };

        _context.AuthTokens.Add(token);
        await _context.SaveChangesAsync(cancellationToken);
        return result.ToString();
    }
}
