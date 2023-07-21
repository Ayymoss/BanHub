using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using BanHub.Configuration;
using BanHub.Models;
using BanHub.Services;
using BanHubData.Commands.Instance;
using BanHubData.Commands.Instance.Server;
using BanHubData.Commands.Penalty;
using BanHubData.Commands.Player;
using BanHubData.Enums;
using Microsoft.Extensions.Logging;
using SharedLibraryCore;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace BanHub.Managers;

public class EndpointManager
{
    public readonly ConcurrentDictionary<EFClient, string> Profiles = new();
    private readonly ILogger<EndpointManager> _logger;
    private readonly NoteService _noteService;
    private readonly PlayerService _player;
    private readonly BanHubConfiguration _banHubConfiguration;
    private readonly CommunitySlim _communitySlim;
    private readonly CommunityService _community;
    private readonly PenaltyService _penalty;
    private readonly ServerService _server;

    public EndpointManager(BanHubConfiguration banHubConfiguration, CommunitySlim communitySlim,
        PlayerService playerService, CommunityService communityService, PenaltyService penaltyService,
        ServerService serverService, ILogger<EndpointManager> logger, NoteService noteService)
    {
        _banHubConfiguration = banHubConfiguration;
        _communitySlim = communitySlim;
        _player = playerService;
        _community = communityService;
        _penalty = penaltyService;
        _server = serverService;
        _logger = logger;
        _noteService = noteService;
    }

    public async Task<bool> CreateOrUpdateCommunityAsync(CreateOrUpdateCommunityCommand community) =>
        await _community.CreateOrUpdateCommunityAsync(community);

    public async Task<bool> IsCommunityActive(Guid guid) => await _community.IsCommunityActiveAsync(guid.ToString());

    private static string EntityToPlayerIdentity(EFClient client) => $"{client.GuidString}:{client.GameName.ToString()}";

    public async Task OnStart(CreateOrUpdateServerCommand server)
    {
        if (!Plugin.CommunityActive) return;
        await _server.PostServer(server);
    }

    public async Task OnJoin(EFClient player)
    {
        // We don't want to act on anything if they're not authenticated
        if (!_communitySlim.Active) return;

        var result = await CreateOrUpdatePlayerAsync(player);
        if (!result.Success) return;

        var isBanned = await _player.IsPlayerBannedAsync(new IsPlayerBannedCommand
        {
            Identity = result.Identity,
            IpAddress = player.IPAddressString
        });

        if (isBanned)
        {
            ProcessPlayer(player);
            return;
        }

        var noteCount = await _noteService.GetUserNotesCountAsync(EntityToPlayerIdentity(player));
        if (noteCount is not 0)
            InformAdmins(player.CurrentServer,
                _banHubConfiguration.Translations.UserHasNotes
                    .FormatExt(_banHubConfiguration.Translations.BanHubName, player.Name, noteCount));


        Profiles.TryAdd(player, result.Identity);
    }

    private async Task<(bool Success, string Identity)> CreateOrUpdatePlayerAsync(EFClient player)
    {
        var createOrUpdate = new CreateOrUpdatePlayerCommand
        {
            PlayerIdentity = $"{player.GuidString}:{player.GameName.ToString()}",
            PlayerAliasUserName = player.CleanedName,
            PlayerAliasIpAddress = player.ClientId is not 0 ? player.IPAddressString : "0.0.0.0",
            PlayerCommunityRole = player.ClientPermission.Level switch
            {
                Data.Models.Client.EFClient.Permission.Trusted => CommunityRole.Trusted,
                Data.Models.Client.EFClient.Permission.Moderator => CommunityRole.Moderator,
                Data.Models.Client.EFClient.Permission.Administrator => CommunityRole.Administrator,
                Data.Models.Client.EFClient.Permission.SeniorAdmin => CommunityRole.SeniorAdmin,
                Data.Models.Client.EFClient.Permission.Owner => CommunityRole.Owner,
                _ => CommunityRole.User
            },
            CommunityGuid = _communitySlim.CommunityGuid,
            ServerId = player.CurrentServer?.Id
        };

        if (await _player.CreateOrUpdatePlayerAsync(createOrUpdate) is { } identity) return (true, identity);

        _logger.LogError("Failed to update entity {Identity}", createOrUpdate.PlayerIdentity);
        return (false, string.Empty);
    }

    private void ProcessPlayer(EFClient client)
    {
        client.Kick("^1Globally banned!^7\nBanHub.gg",
            SharedLibraryCore.Utilities.IW4MAdminClient(client.CurrentServer));
        _logger.LogInformation("{Name} globally banned", client.CleanedName);
    }

    public void RemoveFromProfiles()
    {
        foreach (var player in Profiles)
        {
            if (player.Key.IsIngame) continue;
            var canRemoveClient = Profiles.TryRemove(player.Key, out _);
            if (canRemoveClient)
            {
                _logger.LogInformation("Removed {Name} from profiles", player.Key.CleanedName);
                return;
            }

            _logger.LogError("Failed to remove {Name} from profiles", player.Key.CleanedName);
        }
    }

    public async Task<(bool, Guid?)> AddPlayerPenaltyAsync(string sourcePenaltyType, EFClient origin, EFClient target,
        string reason, DateTimeOffset? expiration = null, PenaltyScope? scope = null)
    {
        var adminIdentity = await CreateOrUpdatePlayerAsync(origin);
        var targetIdentity = await CreateOrUpdatePlayerAsync(target);
        if (!adminIdentity.Success || !targetIdentity.Success) return (false, null);

        var parsedPenaltyType = Enum.TryParse<PenaltyType>(sourcePenaltyType, out var penaltyType);
        if (!parsedPenaltyType || !Plugin.CommunityActive) return (false, null);
        if (penaltyType is not PenaltyType.Ban && origin.ClientId is 1) return (false, null);

        var (isGlobalAntiCheatBan, isAntiCheatBan) = (false, false);
        var antiCheatReason = origin.AdministeredPenalties?.FirstOrDefault()?.AutomatedOffense;
        if (antiCheatReason is not null)
        {
            const string regex = @"^(Recoil|Button)(-{1,2})(\d{0,})@(\d{0,})$";
            isGlobalAntiCheatBan = Regex.IsMatch(antiCheatReason, regex);
            isAntiCheatBan = true;
        }

        var penaltyDto = new AddPlayerPenaltyCommand
        {
            PenaltyType = penaltyType,
            PenaltyScope = isGlobalAntiCheatBan ? PenaltyScope.Global : scope ?? PenaltyScope.Local,
            Reason = isAntiCheatBan ? antiCheatReason ?? "AntiCheat Detection" : reason.StripColors(),
            Automated = isAntiCheatBan,
            Expiration = expiration,
            CommunityGuid = _communitySlim.CommunityGuid,
            AdminIdentity = adminIdentity.Identity,
            TargetIdentity = targetIdentity.Identity
        };
        var result = await _penalty.AddPlayerPenaltyAsync(penaltyDto);

        if (_banHubConfiguration.PrintPenaltyToConsole)
        {
            var guid = result.Item1 ? $"GUID: {result.Item2.ToString()}" : "Error creating penalty!";
            Console.WriteLine(
                $"[{BanHubConfiguration.Name} - {DateTimeOffset.UtcNow:HH:mm:ss}] {penaltyType} ({penaltyDto.PenaltyScope}): " +
                $"{origin.CleanedName} -> {target.CleanedName} ({penaltyDto.Reason}) - {guid}");
        }

        return result;
    }

    public async Task<bool> AddPlayerPenaltyEvidenceAsync(Guid guid, string evidence, EFClient issuer, EFClient offender)
    {
        var penalty = new AddPlayerPenaltyEvidenceCommand
        {
            PenaltyGuid = guid,
            Evidence = evidence,
            IssuerIdentity = EntityToPlayerIdentity(issuer),
            IssuerUsername = issuer.CleanedName,
            OffenderIdentity = EntityToPlayerIdentity(offender),
            OffenderUsername = offender.CleanedName
        };
        return await _penalty.SubmitEvidence(penalty);
    }

    public async Task<string?> GetTokenAsync(EFClient client)
    {
        var identity = EntityToPlayerIdentity(client);
        return await _player.GetTokenAsync(new GetPlayerTokenCommand {Identity = identity});
    }

    private static void InformAdmins(Server server, string message)
    {
        var admins = server.GetClientsAsList().Where(x => x.IsPrivileged());
        foreach (var admin in admins)
        {
            admin.Tell(message);
        }
    }
}
