using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using BanHub.Configuration;
using BanHub.Enums;
using BanHub.Models;
using BanHub.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedLibraryCore;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace BanHub.Managers;

public class EndpointManager
{
    public readonly ConcurrentDictionary<EFClient, EntityDto> Profiles = new();

    private readonly ILogger<EndpointManager> _logger;
    private readonly EntityEndpoint _entity;
    private readonly ConfigurationModel _configurationModel;
    private readonly InstanceDto _instanceMeta;
    private readonly InstanceEndpoint _instance;
    private readonly PenaltyEndpoint _penalty;
    private readonly ServerEndpoint _server;

    public EndpointManager(IServiceProvider serviceProvider, ConfigurationModel configurationModel, InstanceDto instanceMeta,
        EntityEndpoint entityEndpoint, InstanceEndpoint instanceEndpoint, PenaltyEndpoint penaltyEndpoint, ServerEndpoint serverEndpoint)
    {
        _configurationModel = configurationModel;
        _instanceMeta = instanceMeta;
        _entity = entityEndpoint;
        _instance = instanceEndpoint;
        _penalty = penaltyEndpoint;
        _server = serverEndpoint;
        _logger = serviceProvider.GetRequiredService<ILogger<EndpointManager>>();
    }

    private static string GetIdentity(EFClient client) => $"{client.GuidString}:{client.GameName.ToString()}";
    public async Task<bool> UpdateInstance(InstanceDto instance) => await _instance.PostInstance(instance);
    public async Task<bool> IsInstanceActive(InstanceDto instance) => await _instance.IsInstanceActive(instance.InstanceGuid);

    private EntityDto ClientToEntity(EFClient client)
    {
        var entity = new EntityDto
        {
            Identity = GetIdentity(client),
            Alias = new AliasDto
            {
                UserName = client.CleanedName,
                IpAddress = client.IPAddressString
            },
            Instance = _instanceMeta,
            InstanceRole = client.ClientPermission.Name switch
            {
                "Trusted" => InstanceRole.InstanceTrusted,
                "Moderator" => InstanceRole.InstanceModerator,
                "Administrator" => InstanceRole.InstanceAdministrator,
                "SeniorAdmin" => InstanceRole.InstanceSeniorAdmin,
                "Owner" => InstanceRole.InstanceOwner,
                _ => InstanceRole.InstanceUser
            }
        };

        if (!client.IsIngame) return entity;

        var server = (IGameServer)client.CurrentServer;
        var serverDto = new ServerDto
        {
            ServerId = server.Id,
            ServerName = server.ServerName,
            ServerIp = server.ListenAddress,
            ServerPort = server.ListenPort
        };

        entity.Server = serverDto;
        return entity;
    }

    public async Task OnStart(ServerDto server)
    {
        if (!Plugin.InstanceActive) return;
        await _server.PostServer(server);
    }

    public async Task OnJoin(EFClient client)
    {
        var entity = ClientToEntity(client);
        var returnedEntity = await _entity.GetEntity(entity.Identity);

        // Action any penalties
        if (returnedEntity is not null)
        {
            ProcessEntity(returnedEntity, client); // TODO: Remove tag
            Profiles.TryAdd(client, returnedEntity);
        }

        // We don't want to act on anything if they're not authenticated
        if (!_instanceMeta.Active!.Value) return;

        var entityUpdated = await _entity.UpdateEntity(entity);

        // If they're new we need to add them to the profiles
        if (returnedEntity is null && entityUpdated)
        {
            var newEntity = await _entity.GetEntity(entity.Identity);
            if (newEntity is not null)
            {
                ProcessEntity(newEntity, client); // TODO: Remove tag
                Profiles.TryAdd(client, newEntity);
            }
        }
    }

    private void ProcessEntity(EntityDto entity, EFClient client)
    {
        var globalBan = entity.Penalties?
            .Any(x => x is {PenaltyType: PenaltyType.Ban, PenaltyScope: PenaltyScope.Global, PenaltyStatus: PenaltyStatus.Active}) ?? false;

        if (!entity.HasIdentityBan!.Value && (!globalBan || !client.IsIngame)) return;
        
        client.Kick("^1Globally banned!^7\nBanHub.gg", Utilities.IW4MAdminClient(client.CurrentServer));
        _logger.LogInformation("{Name} globally banned. Referenced? {Association}", client.CleanedName, entity.HasIdentityBan);
    }

    public void RemoveFromProfiles(EFClient client)
    {
        var remove = Profiles.TryRemove(client, out _);
        if (remove)
        {
            _logger.LogInformation("Removed {Name} from profiles", client.CleanedName);
            return;
        }

        _logger.LogError("Failed to remove {Name} from profiles", client.CleanedName);
    }

    public async Task<(bool, Guid?)> NewPenalty(string sourcePenaltyType, EFClient origin, EFClient target,
        string reason, TimeSpan? duration = null, PenaltyScope? scope = null, string? evidence = null)
    {
        var parsedPenaltyType = Enum.TryParse<PenaltyType>(sourcePenaltyType, out var penaltyType);
        if (!parsedPenaltyType || !Plugin.InstanceActive) return (false, null);
        if (penaltyType is PenaltyType.Kick or PenaltyType.Warning && origin.ClientId == 1) return (false, null);

        var antiCheatReason = origin.AdministeredPenalties?.FirstOrDefault()?.AutomatedOffense;
        var globalAntiCheatBan = false;

        if (antiCheatReason is not null)
        {
            const string regex = @"^(Recoil|Button)(-{1,2})(\d{0,})@(\d{0,})$";
            globalAntiCheatBan = Regex.IsMatch(antiCheatReason, regex);
        }

        var adminEntity = ClientToEntity(origin);
        var targetEntity = ClientToEntity(target);

        var penaltyDto = new PenaltyDto
        {
            PenaltyType = penaltyType,
            PenaltyScope = globalAntiCheatBan ? PenaltyScope.Global : scope ?? PenaltyScope.Local,
            Evidence = evidence,
            Reason = globalAntiCheatBan ? "AntiCheat Detection" : reason,
            AntiCheatReason = antiCheatReason,
            Duration = duration is not null && duration.Value.TotalSeconds > 1 ? duration : null,
            Instance = _instanceMeta,
            Admin = adminEntity,
            Target = targetEntity
        };
        var result = await _penalty.PostPenalty(penaltyDto);

        if (_configurationModel.PrintPenaltyToConsole)
        {
            var guid = result.Item1 ? $"GUID: {result.Item2.ToString()}" : "Error creating penalty!";
            Console.WriteLine(
                $"[{ConfigurationModel.Name} - {DateTimeOffset.UtcNow:HH:mm:ss}] {penaltyType} ({penaltyDto.PenaltyScope}): " +
                $"{origin.CleanedName} -> {target.CleanedName} ({penaltyDto.Reason}) - {guid}");
        }

        return result;
    }

    public async Task<bool> SubmitInformation(Guid guid, string evidence)
    {
        var penalty = new PenaltyDto
        {
            PenaltyGuid = guid,
            Evidence = evidence
        };
        return await _penalty.SubmitEvidence(penalty);
    }

    public async Task<string?> GenerateToken(EFClient client)
    {
        var entity = ClientToEntity(client);
        return await _entity.GetToken(entity);
    }
}
