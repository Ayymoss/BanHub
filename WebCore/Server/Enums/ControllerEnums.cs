namespace BanHub.WebCore.Server.Enums;

public abstract class ControllerEnums
{
    public enum ProfileReturnState
    {
        Updated,
        Created,
        NotFound,
        BadRequest,
        Conflict,
        Accepted,
        Ok,
        NotModified,
        Unauthorized
    }

    public enum StatisticType
    {
        EntityCount = -4,
        ServerCount = -3,
        InfractionCount = -2,
        InstanceCount = -1
    }
}
