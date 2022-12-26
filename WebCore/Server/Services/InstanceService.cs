using GlobalInfraction.WebCore.Server.Context;
using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Server.Interfaces;
using GlobalInfraction.WebCore.Server.Models;
using GlobalInfraction.WebCore.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GlobalInfraction.WebCore.Server.Services;

public class InstanceService : IInstanceService
{
    private readonly SqliteDataContext _context;

    public InstanceService(SqliteDataContext context)
    {
        _context = context;
    }

    public async Task<(ControllerEnums.ProfileReturnState, string)> CreateOrUpdate(InstanceDto request, string? requestIpAddress)
    {
        // TODO: There should only be one instance per IP address. Check for this and return an error if it exists.
        var instanceGuid = await _context.Instances
            .AsTracking()
            .FirstOrDefaultAsync(server => server.InstanceGuid == request.InstanceGuid);
        var instanceApi = await _context.Instances
            .FirstOrDefaultAsync(server => server.ApiKey == request.ApiKey);

        var ipAddress = requestIpAddress ?? request.InstanceIp;

        // New instance
        if (instanceApi is null && instanceGuid is null)
        {
            _context.Instances.Add(new EFInstance
            {
                InstanceGuid = request.InstanceGuid,
                InstanceIp = ipAddress!,
                InstanceName = request.InstanceName,
                ApiKey = request.ApiKey!.Value,
                Active = false,
                HeartBeat = DateTimeOffset.UtcNow
            });

            await _context.SaveChangesAsync();

            return (ControllerEnums.ProfileReturnState.Created, $"Instance added {request.InstanceGuid}");
        }

        // Check existing record
        if (instanceGuid is null || instanceApi is null) return (ControllerEnums.ProfileReturnState.BadRequest, "GUID + API mismatch");
        if (instanceGuid.Id != instanceApi.Id)
            return (ControllerEnums.ProfileReturnState.Conflict, "Instance already exists with this API key.");

        // Warn if IP address has changed... this really shouldn't happen.
        if (requestIpAddress is null || requestIpAddress != instanceGuid.InstanceIp)
        {
            // TODO: Maybe webhook this?
            //_logger.LogWarning("{Instance} IP mismatch! Request: [{ReqIP}], Registered: [{InstanceIP}]",
            //    instanceGuid.InstanceGuid, requestIpAddress, instanceGuid.InstanceIp);
        }

        // Update existing record
        instanceGuid.HeartBeat = DateTimeOffset.UtcNow;
        instanceGuid.InstanceName = request.InstanceName;
        _context.Instances.Update(instanceGuid);
        await _context.SaveChangesAsync();

        return instanceGuid.Active
            ? (ControllerEnums.ProfileReturnState.Accepted, "Instance exists, and is active.")
            : (ControllerEnums.ProfileReturnState.Ok, "Instance exists, but is not active.");
    }

    public async Task<(ControllerEnums.ProfileReturnState, InstanceDto?)> GetServer(string guid)
    {
        var guidParse = Guid.TryParse(guid, out var guidResult);
        if (!guidParse) return (ControllerEnums.ProfileReturnState.BadRequest, null);

        var result = await _context.Instances.SingleOrDefaultAsync(x => x.InstanceGuid == guidResult);
        if (result is null) return (ControllerEnums.ProfileReturnState.NotFound, null);

        return (ControllerEnums.ProfileReturnState.Ok, new InstanceDto
        {
            InstanceGuid = result.InstanceGuid,
            InstanceIp = result.InstanceIp,
            InstanceName = result.InstanceName,
            Active = result.Active
        });
    }
}
