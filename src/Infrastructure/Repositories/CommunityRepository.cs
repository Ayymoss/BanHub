using BanHub.Domain.Entities;
using BanHub.Domain.Interfaces.Repositories;
using BanHub.Domain.ValueObjects.Repository.Community;
using BanHub.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BanHub.Infrastructure.Repositories;

public class CommunityRepository(DataContext context) : ICommunityRepository
{
    public async Task<EFCommunity?> GetCommunityAsync(Guid communityGuid, CancellationToken cancellationToken)
    {
        return await context.Communities
            .Where(x => x.CommunityGuid == communityGuid)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task<bool> IsCommunityApiKeyValidAsync(Guid communityGuid, Guid apiKey, CancellationToken cancellationToken)
    {
        return await context.Communities
            .Where(x => x.CommunityGuid == communityGuid)
            .Where(x => x.ApiKey == apiKey)
            .AnyAsync(cancellationToken: cancellationToken);
    }

    public async Task<int> AddOrUpdateCommunityAsync(EFCommunity community, CancellationToken cancellationToken)
    {
        var efCommunity = await context.Communities
            .Where(x => x.Id == community.Id)
            .AnyAsync(cancellationToken: cancellationToken);

        if (efCommunity) context.Communities.Update(community);
        else context.Communities.Add(community);

        await context.SaveChangesAsync(cancellationToken);
        return community.Id;
    }

    public async Task<int?> GetCommunityIdAsync(Guid communityGuid, CancellationToken cancellationToken)
    {
        var community = await context.Communities
            .Where(x => x.CommunityGuid == communityGuid)
            .Select(x => new {x.Id})
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return community?.Id;
    }

    public async Task<int> GetCommunityCountAsync(CancellationToken cancellationToken)
    {
        return await context.Communities.CountAsync(cancellationToken: cancellationToken);
    }

    public async Task<bool> IsCommunityActiveAsync(Guid communityGuid, CancellationToken cancellationToken)
    {
        var result = await context.Communities
            .Where(x => x.CommunityGuid == communityGuid)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken) is {Active: true};
        return result;
    }

    public async Task<CommunityView?> GetCommunityProfileAsync(Guid communityGuid, CancellationToken cancellationToken)
    {
        var result = await context.Communities
            .Where(x => x.CommunityGuid == communityGuid)
            .Select(x => new CommunityView
            {
                CommunityGuid = x.CommunityGuid,
                CommunityIpFriendly = x.CommunityIpFriendly,
                CommunityPort = x.CommunityPort,
                CommunityName = x.CommunityName,
                CommunityIp = x.CommunityIp,
                About = x.About,
                Socials = x.Socials,
                Active = x.Active,
                HeartBeat = x.HeartBeat,
                Created = x.Created,
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return result;
    }
}
