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

    public EndpointManager(IServiceProvider serviceProvider, ConfigurationModel configurationModel, InstanceDto instanceMeta)
    {
        _configurationModel = configurationModel;
        _instanceMeta = instanceMeta;
        _entity = new EntityEndpoint(configurationModel);
        _instance = new InstanceEndpoint(configurationModel);
        _penalty = new PenaltyEndpoint(configurationModel);
        _server = new ServerEndpoint(configurationModel);
        _logger = serviceProvider.GetRequiredService<ILogger<EndpointManager>>();
    }

    private static string GetIdentity(EFClient client) => $"{client.GuidString}:{client.GameName.ToString()}";

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
            Instance = _instanceMeta
        };

        if (!client.IsIngame) return entity;

        var server = new ServerDto
        {
            ServerId = $"{client.CurrentServer.IP}:{client.CurrentServer.Port}",
            ServerName = client.CurrentServer.Hostname,
            ServerIp = client.CurrentServer.IP,
            ServerPort = client.CurrentServer.Port
        };

        entity.Server = server;
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
            .Any(x => x.PenaltyType is PenaltyType.Ban && x.PenaltyScope is PenaltyScope.Global && x.PenaltyStatus == PenaltyStatus.Active) ?? false;

        if (globalBan && client.IsIngame)
        {
            client.Kick("^1Globally banned!^7\nBanHub.gg", Utilities.IW4MAdminClient(client.CurrentServer));
        }
    }

    public async Task<bool> UpdateInstance(InstanceDto instance)
    {
        return await _instance.PostInstance(instance);
    }

    public async Task<bool> IsInstanceActive(InstanceDto instance)
    {
        return await _instance.IsInstanceActive(instance.InstanceGuid);
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

    public async Task<(bool, Guid?)> NewPenalty(PenaltyType penaltyType, EFClient origin, EFClient target,
        string reason, TimeSpan? duration = null, PenaltyScope? scope = null, string? evidence = null)
    {
        if (!Plugin.InstanceActive) return (false, null);
        if (penaltyType is PenaltyType.Kick or PenaltyType.Warn && origin.ClientId == 1) return (false, null);

        var penalty = origin.AdministeredPenalties?.FirstOrDefault()?.AutomatedOffense;
        var automatedBan = false;
        var automatedReason = string.Empty;
        if (penalty is not null)
        {
            const string regex = @"^(Recoil|Button)(-{1,2})(\d{0,})@(\d{0,})$";
            automatedBan = Regex.IsMatch(penalty, regex);
            var match = Regex.Match(penalty, regex).Groups[1].ToString();
            automatedReason = match switch
            {
                "Recoil" => "R",
                "Button" => "B",
                _ => "??"
            };
        }

        var adminEntity = ClientToEntity(origin);
        var targetEntity = ClientToEntity(target);

        var penaltyDto = new PenaltyDto
        {
            PenaltyType = penaltyType,
            PenaltyScope = automatedBan ? PenaltyScope.Global : scope ?? PenaltyScope.Local,
            Evidence = evidence,
            Reason = automatedBan ? $"Automated Offense [{automatedReason}]" : reason,
            Duration = duration,
            Instance = _instanceMeta,
            Admin = adminEntity,
            Target = targetEntity
        };
        var result = await _penalty.PostPenalty(penaltyDto);

        if (_configurationModel.PrintPenaltyToConsole)
        {
            var guid = result.Item1 ? $"GUID: {result.Item2.ToString()}" : "Error creating penalty!";
            Console.WriteLine(
                $"[{Plugin.PluginName} - {DateTimeOffset.UtcNow:HH:mm:ss}] {penaltyType} ({penaltyDto.PenaltyScope}): " +
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
