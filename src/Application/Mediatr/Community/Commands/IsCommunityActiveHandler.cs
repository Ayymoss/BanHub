using BanHub.Domain.Interfaces.Repositories;
using MediatR;

namespace BanHub.Application.Mediatr.Community.Commands;

public class IsCommunityActiveHandler(ICommunityRepository communityRepository) : IRequestHandler<IsCommunityActiveCommand, bool>
{
    public async Task<bool> Handle(IsCommunityActiveCommand request, CancellationToken cancellationToken)
    {
        return await communityRepository.IsCommunityActiveAsync(request.CommunityGuid, cancellationToken);
    }
}
