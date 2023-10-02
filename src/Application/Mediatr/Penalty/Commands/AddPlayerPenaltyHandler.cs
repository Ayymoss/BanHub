using BanHub.Application.Mediatr.Services.Events.Discord;
using BanHub.Application.Mediatr.Services.Events.Statistics;
using BanHub.Domain.Entities;
using BanHub.Domain.Enums;
using BanHub.Domain.Interfaces.Repositories;
using BanHub.Domain.Interfaces.Services;
using BanHub.Domain.ValueObjects.Plugin.SignalR;
using BanHub.Domain.ValueObjects.Repository.Player;
using MediatR;

namespace BanHub.Application.Mediatr.Penalty.Commands;

public class AddPlayerPenaltyHandler(ISignalRNotification signalRNotification, IPublisher publisher, IPlayerRepository playerRepository,
        IPenaltyRepository penaltyRepository, ICommunityRepository communityRepository,
        IPenaltyIdentifierRepository penaltyIdentifierRepository)
    : IRequestHandler<AddPlayerPenaltyCommand, (ControllerEnums.ReturnState, Guid?)>
{
    // TODO: Implement this "Unit of Work" concept. https://chat.openai.com/c/8489bcd7-06ac-4e1c-8c27-a65806b5e7a6
    public async Task<(ControllerEnums.ReturnState, Guid?)> Handle(AddPlayerPenaltyCommand request, CancellationToken cancellationToken)
    {
        var target = await playerRepository.GetPlayerIdAsync(request.TargetIdentity, cancellationToken);
        var admin = await playerRepository.GetPlayerIdAsync(request.AdminIdentity, cancellationToken);
        var community = await communityRepository.GetCommunityAsync(request.CommunityGuid, cancellationToken);

        if (target is null || admin is null || community is null)
            return (ControllerEnums.ReturnState.BadRequest, null);

        var penalties = await penaltyRepository.CurrentActivePenaltiesByCommunityAsync(request.CommunityGuid, target.Id, cancellationToken);
        var hasExistingGlobalBan = await penaltyRepository.HasActiveGlobalBanAsync(target.Id, cancellationToken);

        var efPenalties = penalties as EFPenalty[] ?? penalties.ToArray();
        switch (request.PenaltyType)
        {
            // Revoke existing bans from the same instance
            case PenaltyType.Unban when efPenalties.Length is not 0:
            {
                var updatePenalties = Array.Empty<EFPenalty>();
                var removeIdentifiers = Array.Empty<EFPenaltyIdentifier>();
                foreach (var inf in efPenalties)
                {
                    inf.PenaltyStatus = PenaltyStatus.Revoked;
                    updatePenalties = updatePenalties.Append(inf).ToArray();
                    if (inf.Identifier is not null) removeIdentifiers = removeIdentifiers.Append(inf.Identifier).ToArray();
                }

                await penaltyRepository.UpdatePenaltiesAsync(updatePenalties, cancellationToken);
                await penaltyIdentifierRepository.RemovePenaltyIdentifiersAsync(removeIdentifiers, cancellationToken);
                break;
            }
            // If they're kicked for an already existing ban, ignore.
            case PenaltyType.Kick when efPenalties.Length is not 0:
                return (ControllerEnums.ReturnState.Conflict, null);
            // Unban when they have no penalties?! Back out.
            case PenaltyType.Unban when efPenalties.Length is 0:
                return (ControllerEnums.ReturnState.NotFound, null);
            // Back out if they already have a global ban
            case PenaltyType.Ban when hasExistingGlobalBan && request.PenaltyScope is PenaltyScope.Global:
                return (ControllerEnums.ReturnState.Conflict, null);
        }

        var penaltyModel = CreatePenalty(request, admin, community, target);
        await penaltyRepository.AddOrUpdatePenaltyAsync(penaltyModel, cancellationToken);

        if (request is {PenaltyType: PenaltyType.Ban, PenaltyScope: PenaltyScope.Global})
            await CreateNewPenaltyIdentifierAsync(target, penaltyModel, cancellationToken);

        await HandleNotificationsAsync(request, target, penaltyModel, community, cancellationToken);
        return (ControllerEnums.ReturnState.Created, penaltyModel.PenaltyGuid);
    }

    private async Task HandleNotificationsAsync(AddPlayerPenaltyCommand request, PlayerInfo target, EFPenalty penaltyModel,
        EFCommunity community, CancellationToken cancellationToken)
    {
        if (request.PenaltyScope is PenaltyScope.Global)
        {
            await signalRNotification.NotifyUserAsync(HubType.Plugin, SignalRMethods.PluginMethods.OnGlobalBan, new BroadcastGlobalBan
            {
                Identity = target.Identity,
                UserName = target.UserName,
            }, cancellationToken: cancellationToken);
            await publisher.Publish(new UpdateRecentBansNotification
            {
                BanGuid = penaltyModel.PenaltyGuid,
                Submitted = DateTimeOffset.UtcNow
            }, cancellationToken);
            await penaltyRepository.AddOrUpdatePenaltyAsync(penaltyModel, cancellationToken);
        }

        await publisher.Publish(new UpdateStatisticsNotification
        {
            StatisticType = ControllerEnums.StatisticType.PenaltyCount,
            StatisticTypeAction = ControllerEnums.StatisticTypeAction.Add
        }, cancellationToken);

        if (request is {PenaltyType: PenaltyType.Ban, PenaltyScope: PenaltyScope.Global})
            await publisher.Publish(new CreatePenaltyNotification
            {
                Scope = penaltyModel.PenaltyScope,
                PenaltyType = penaltyModel.PenaltyType,
                PenaltyGuid = penaltyModel.PenaltyGuid,
                TargetIdentity = target.Identity,
                Username = target.UserName,
                Reason = request.Automated ? "Automated Detection" : penaltyModel.Reason,
                CommunityGuid = community.CommunityGuid,
                CommunityName = community.CommunityName
            }, cancellationToken);
    }

    private async Task CreateNewPenaltyIdentifierAsync(PlayerInfo target, EFPenalty penaltyModel, CancellationToken cancellationToken)
    {
        var identifier = new EFPenaltyIdentifier
        {
            Identity = target.Identity,
            IpAddress = target.IpAddress,
            Expiration = DateTimeOffset.UtcNow.AddMonths(1),
            PenaltyId = penaltyModel.Id,
        };
        await penaltyIdentifierRepository.AddPenaltyIdentifierAsync(identifier, cancellationToken);
    }

    private static EFPenalty CreatePenalty(AddPlayerPenaltyCommand request, PlayerInfo admin, EFCommunity community, PlayerInfo target)
    {
        return new EFPenalty
        {
            PenaltyType = request.PenaltyType,
            PenaltyStatus = request.PenaltyType is PenaltyType.Ban or PenaltyType.TempBan
                ? PenaltyStatus.Active
                : PenaltyStatus.Informational,
            PenaltyScope = request.PenaltyScope,
            PenaltyGuid = Guid.NewGuid(),
            Submitted = DateTimeOffset.UtcNow,
            IssuerId = admin.Id,
            Reason = request.Reason,
            Automated = request.Automated,
            Expiration = request.Expiration,
            CommunityId = community.Id,
            RecipientId = target.Id
        };
    }
}
