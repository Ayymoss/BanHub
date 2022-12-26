namespace GlobalInfraction.WebCore.Server.Enums;

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
        NotModified
    }
}
