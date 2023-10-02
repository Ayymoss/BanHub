namespace BanHub.Domain.Enums;

public abstract class ControllerEnums
{
    public enum ReturnState
    {
        Updated,
        Created,
        NotFound,
        BadRequest,
        Conflict,
        Accepted,
        Ok,
        Unauthorized,
        NoContent
    }

    public enum StatisticType
    {
        PlayerCount,
        ServerCount,
        PenaltyCount,
        CommunityCount
    }

    public enum StatisticTypeAction
    {
        Add,
        Subtract
    }
}
