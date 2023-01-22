using GlobalInfraction.WebCore.Shared.Enums;

namespace GlobalInfraction.WebCore.Shared.DTOs.WebEntity;

public class UserDto
{
    public string UserName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string Token { get; set; } = null!;
    public int ExpiresIn { get; set; }
    public DateTimeOffset ExpiryTime { get; set; }
}
