using BanHub.Domain.Interfaces.Repositories;
using MediatR;

namespace BanHub.Application.Mediatr.Community.Commands;

public class GetCommunityHandler(IServerRepository serverRepository, ICommunityRepository communityRepository,
        IPenaltyRepository penaltyRepository)
    : IRequestHandler<GetCommunityCommand, DTOs.WebView.CommunityProfileView.Community?>
{
    public async Task<DTOs.WebView.CommunityProfileView.Community?> Handle(GetCommunityCommand request,
        CancellationToken cancellationToken)
    {
        var community = await communityRepository.GetCommunityProfileAsync(request.CommunityGuid, cancellationToken);
        if (community is null) return null;

        var manual = await penaltyRepository.GetCommunityPenaltiesCountAsync(request.CommunityGuid, false, cancellationToken);
        var automated = await penaltyRepository.GetCommunityPenaltiesCountAsync(request.CommunityGuid, true, cancellationToken);
        var serverCount = await serverRepository.GetServerCountAsync(request.CommunityGuid, cancellationToken);

        var result = new DTOs.WebView.CommunityProfileView.Community
        {
            CommunityGuid = community.CommunityGuid,
            CommunityWebsite = community.CommunityIpFriendly ?? $"{community.CommunityIp}:{community.CommunityPort}",
            CommunityPort = community.CommunityPort,
            CommunityName = community.CommunityName,
            About = community.About,
            Socials = community.Socials,
            Active = community.Active,
            Connected = community.HeartBeat + TimeSpan.FromMinutes(5) > DateTimeOffset.UtcNow,
            Created = community.Created,
            ServerCount = serverCount,
            AutomatedPenaltiesCount = automated,
            PenaltiesCount = automated + manual,
        };

        return result;
    }
}
