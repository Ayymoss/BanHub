using System.Text;
using BanHub.Domain.Entities;
using BanHub.Domain.Interfaces.Repositories;
using MediatR;

namespace BanHub.Application.Mediatr.Player.Commands;

public class GetPlayerTokenHandler(IPlayerRepository playerRepository, IAuthTokenRepository authTokenRepository)
    : IRequestHandler<GetPlayerTokenCommand, string?>
{
    public async Task<string?> Handle(GetPlayerTokenCommand request, CancellationToken cancellationToken)
    {
        var player = await playerRepository.GetPlayerByIdentityAsync(request.Identity, cancellationToken);
        if (player is null) return null;

        var utcTimeNow = DateTimeOffset.UtcNow;

        var hasActiveToken = await authTokenRepository.GetActiveTokenByPlayerIdAsync(player.Id, cancellationToken);
        if (hasActiveToken is not null) return hasActiveToken.Token;

        // ReSharper disable StringLiteralTypo
        const string characters = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ23456789";
        // ReSharper restore StringLiteralTypo

        var result = new StringBuilder(6);
        for (var i = 0; i < 6; i++) result.Append(characters[Random.Shared.Next(characters.Length)]);

        var token = new EFAuthToken
        {
            Token = result.ToString(),
            Expiration = utcTimeNow + TimeSpan.FromMinutes(5),
            PlayerId = player.Id,
            Used = false
        };

        await authTokenRepository.CreateNewAuthTokenAsync(token, cancellationToken);
        return result.ToString();
    }
}
