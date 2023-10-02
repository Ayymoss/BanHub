using BanHub.Domain.Interfaces.Repositories;
using MediatR;

namespace BanHub.Application.Mediatr.Player.Commands;

public class HasIdentityBanHandler
    (IPenaltyIdentifierRepository penaltyIdentifierRepository) : IRequestHandler<HasIdentityBanCommand, bool>
{
    public async Task<bool> Handle(HasIdentityBanCommand request, CancellationToken cancellationToken)
    {
        var hasIdentityBan = await penaltyIdentifierRepository.HasIdentityBanAsync(request.Identity, cancellationToken);
        return hasIdentityBan;
    }
}
