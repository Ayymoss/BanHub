using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using BanHub.Commands;
using BanHub.Configuration;
using BanHub.Models;
using BanHub.Services;
using BanHubData.Commands.Chat;
using BanHubData.Commands.Community;
using BanHubData.Commands.Community.Server;
using BanHubData.Commands.Penalty;
using BanHubData.Commands.Player;
using BanHubData.Enums;
using BanHubData.SignalR;
using Microsoft.Extensions.Logging;
using SharedLibraryCore;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Events.Game;
using SharedLibraryCore.Helpers;
using SharedLibraryCore.Interfaces;

namespace BanHub.Managers;

public class EndpointManager
{
    private readonly SemaphoreSlim _chatLock = new(1, 1);
    public readonly ConcurrentDictionary<EFClient, string> Profiles = new();
    private readonly ILogger<EndpointManager> _logger;
    private readonly NoteService _noteService;
    private readonly ChatService _chatService;
    private readonly IInteractionRegistration _interactionRegistration;
    private readonly IRemoteCommandService _remoteCommandService;
    private readonly PlayerService _playerService;
    private readonly BanHubConfiguration _banHubConfiguration;
    private readonly CommunitySlim _communitySlim;
    private readonly CommunityService _community;
    private readonly PenaltyService _penalty;
    private readonly ServerService _server;
    private const string GlobalBanInteractionLocation = "Webfront::Profile::GlobalBan";

    public EndpointManager(BanHubConfiguration banHubConfiguration, CommunitySlim communitySlim, PlayerService playerService,
        CommunityService communityService, PenaltyService penaltyService, ServerService serverService, ILogger<EndpointManager> logger,
        NoteService noteService, ChatService chatService, IInteractionRegistration interactionRegistration,
        IRemoteCommandService remoteCommandService)
    {
        _banHubConfiguration = banHubConfiguration;
        _communitySlim = communitySlim;
        _playerService = playerService;
        _community = communityService;
        _penalty = penaltyService;
        _server = serverService;
        _logger = logger;
        _noteService = noteService;
        _chatService = chatService;
        _interactionRegistration = interactionRegistration;
        _remoteCommandService = remoteCommandService;
    }

    public async Task<bool> CreateOrUpdateCommunityAsync(CreateOrUpdateCommunityCommand community) =>
        await _community.CreateOrUpdateCommunityAsync(community);

    public async Task<bool> IsCommunityActive(Guid guid) => await _community.IsCommunityActiveAsync(guid.ToString());

    public string EntityToPlayerIdentity(EFClient client) => $"{client.GuidString}:{client.GameName.ToString()}";

    public async Task OnStart(CreateOrUpdateServerCommand server)
    {
        if (!_communitySlim.Active) return;
        await _server.PostServer(server);
    }

    public async Task OnJoin(EFClient player)
    {
        if (!_communitySlim.Active) return;
        var identity = EntityToPlayerIdentity(player);
        var isBanned = await _playerService.IsPlayerBannedAsync(new IsPlayerBannedCommand
        {
            Identity = identity,
            IpAddress = player.IPAddressString
        });

        if (isBanned && !_banHubConfiguration.NotifyOnlyMode)
        {
            ProcessPlayer(player);
            return;
        }

        var result = await CreateOrUpdatePlayerAsync(player);
        if (!result.Success) return;

        var noteCount = await _noteService.GetUserNotesCountAsync(identity);
        if (noteCount is not 0)
            InformAdmins(player.CurrentServer, _banHubConfiguration.Translations.UserHasNotes
                .FormatExt(_banHubConfiguration.Translations.BanHubName, player.Name, noteCount));

        if (isBanned)
        {
            InformAdmins(player.CurrentServer, _banHubConfiguration.Translations.UserIsBanned
                .FormatExt(_banHubConfiguration.Translations.BanHubName, player.Name));
            return;
        }

        Profiles.TryAdd(player, identity);
    }

    private async Task<(bool Success, string Identity)> CreateOrUpdatePlayerAsync(EFClient player)
    {
        if (player.ClientId is 1) return (true, "0:UKN");
        if (!_communitySlim.Active) return (true, EntityToPlayerIdentity(player));

        var createOrUpdate = new CreateOrUpdatePlayerCommand
        {
            PlayerIdentity = $"{player.GuidString}:{player.GameName.ToString()}",
            PlayerAliasUserName = player.CleanedName,
            PlayerAliasIpAddress = player.IPAddressString,
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

        if (await _playerService.CreateOrUpdatePlayerAsync(createOrUpdate) is { } identity) return (true, identity);

        _logger.LogError("Failed to update entity {Identity}", createOrUpdate.PlayerIdentity);
        return (false, string.Empty);
    }

    private void ProcessPlayer(EFClient client)
    {
        client.Kick("^1Globally banned!^7\nBanHub.gg", SharedLibraryCore.Utilities.IW4MAdminClient(client.CurrentServer));
        _logger.LogInformation("{Name} globally banned", client.CleanedName);
    }

    public void RemoveFromProfiles(EFClient client)
    {
        Profiles.TryRemove(client, out _);
        foreach (var player in Profiles)
        {
            if (player.Key.State is EFClient.ClientState.Connected) continue;
            var isRemoved = Profiles.TryRemove(player.Key, out _);
            if (isRemoved) continue;
            _logger.LogError("Failed to remove {Name} from profiles", player.Key.CleanedName);
        }
    }

    public async Task HandleChatMessageAsync(ClientMessageEvent messageEvent, CancellationToken token)
    {
        var message = messageEvent.Message.StripColors();
        if (string.IsNullOrEmpty(message)) return;
        var playerIdentity = EntityToPlayerIdentity(messageEvent.Client);
        var messageContext = new MessageContext(DateTimeOffset.UtcNow, messageEvent.Server.Id, message);

        _communitySlim.PlayerMessages.AddOrUpdate(playerIdentity, new List<MessageContext> {messageContext}, (key, existingMessages) =>
        {
            existingMessages.Add(messageContext);
            return existingMessages;
        });

        try
        {
            await _chatLock.WaitAsync(token);
            if (_communitySlim.PlayerMessages.Count < 25) return;

            var oldMessages = _communitySlim.PlayerMessages;
            _communitySlim.PlayerMessages = new ConcurrentDictionary<string, List<MessageContext>>();

            var communityMessages = new AddCommunityChatMessagesCommand
            {
                CommunityGuid = _communitySlim.CommunityGuid,
                PlayerMessages = oldMessages.ToDictionary(x => x.Key, x => x.Value)
            };

            await _chatService.AddCommunityChatMessagesAsync(communityMessages);
        }
        finally
        {
            if (_chatLock.CurrentCount is 0) _chatLock.Release();
        }
    }

    public async Task<(bool, Guid?)> AddPlayerPenaltyAsync(string sourcePenaltyType, EFClient origin, EFClient target,
        string reason, DateTimeOffset? expiration = null, PenaltyScope? scope = null)
    {
        var adminIdentity = await CreateOrUpdatePlayerAsync(origin);
        var targetIdentity = await CreateOrUpdatePlayerAsync(target);
        if (!adminIdentity.Success || !targetIdentity.Success) return (false, null);

        var parsedPenaltyType = Enum.TryParse<PenaltyType>(sourcePenaltyType, out var penaltyType);
        if (!parsedPenaltyType || !_communitySlim.Active) return (false, null);
        if (penaltyType is not (PenaltyType.Ban or PenaltyType.TempBan or PenaltyType.Unban) && origin.ClientId is 1) return (false, null);

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

        if (_banHubConfiguration.PrintPenaltiesToConsole)
        {
            var guid = result.Item1 ? $"GUID: {result.Item2.ToString()}" : "Error creating penalty!";
            Console.WriteLine(
                $"[{BanHubConfiguration.Name} - {DateTimeOffset.UtcNow:HH:mm:ss}] {penaltyType} ({penaltyDto.PenaltyScope}): " +
                $"{origin.CleanedName} -> {target.CleanedName} ({penaltyDto.Reason}) - {guid}");
        }

        return result;
    }

    public async Task<bool> AddPlayerPenaltyEvidenceAsync(Guid guid, string evidence, EFClient issuer)
    {
        var penalty = new AddPlayerPenaltyEvidenceCommand
        {
            PenaltyGuid = guid,
            Evidence = evidence,
            IssuerIdentity = EntityToPlayerIdentity(issuer),
            IssuerUsername = issuer.CleanedName
        };
        return await _penalty.SubmitEvidence(penalty);
    }

    public async Task<string?> GetTokenAsync(EFClient client)
    {
        var identity = EntityToPlayerIdentity(client);
        return await _playerService.GetTokenAsync(identity);
    }

    private static void InformAdmins(Server server, string message)
    {
        var admins = server.GetClientsAsList().Where(x => x.IsPrivileged());
        foreach (var admin in admins)
        {
            admin.Tell(message);
        }
    }

    public void RegisterInteraction(IManager manager)
    {
        _interactionRegistration.RegisterInteraction(GlobalBanInteractionLocation, async (targetClientId, game, token) =>
        {
            if (!targetClientId.HasValue || !game.HasValue) return null;

            var clientIdentity = EntityToPlayerIdentity(new EFClient
            {
                ClientId = targetClientId.Value,
                GameName = game.Value
            });

            var isGlobalBanned = await _playerService.IsPlayerBannedAsync(clientIdentity);
            var server = manager.GetServers().First();

            string GetCommandName(Type commandType) =>
                manager.Commands.FirstOrDefault(command => command.GetType() == commandType)?.Name ?? string.Empty;

            return isGlobalBanned ? null : CreateGlobalBanInteraction(targetClientId.Value, server, GetCommandName);
        });
    }

    private InteractionData CreateGlobalBanInteraction(int targetClientId, Server server, Func<Type, string> getCommandNameFunc)
    {
        var reasonInput = new
        {
            Name = "Reason",
            Label = "Reason",
            Type = "text",
            Values = (Dictionary<string, string>?)null
        };

        var inputs = new[] {reasonInput};
        var inputsJson = JsonSerializer.Serialize(inputs);

        return new InteractionData
        {
            EntityId = targetClientId,
            Name = "Global Ban",
            DisplayMeta = "oi-ban",
            ActionPath = "DynamicAction",
            ActionMeta = new Dictionary<string, string>
            {
                {"InteractionId", GlobalBanInteractionLocation},
                {"Inputs", inputsJson},
                {
                    "ActionButtonLabel",
                    "Global Ban"
                },
                {
                    "Name",
                    "Global Ban"
                },
                {"ShouldRefresh", true.ToString()}
            },
            MinimumPermission = Data.Models.Client.EFClient.Permission.SeniorAdmin,
            Source = "Ban Hub",
            Action = async (originId, targetId, gameName, meta, cancellationToken) =>
            {
                if (!targetId.HasValue) return "No target client id specified";

                var gBanCommand = getCommandNameFunc(typeof(GlobalBanCommand));
                var args = new List<string>();

                if (meta.TryGetValue(reasonInput.Name, out var reason)) args.Add(reason);

                var commandResponse = await _remoteCommandService.Execute(originId, targetId, gBanCommand, args, server);
                return string.Join(".", commandResponse.Select(result => result.Response));
            }
        };
    }

    public void OnGlobalBan(BroadcastGlobalBan ban, IManager manager)
    {
        var servers = manager.GetServers();

        foreach (var server in servers)
            server.Broadcast(_banHubConfiguration.Translations.BroadcastGlobalBan
                .FormatExt(_banHubConfiguration.Translations.BanHubName, ban.UserName, ban.Identity));
    }
}
