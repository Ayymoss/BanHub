using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Shared.Models;

namespace GlobalInfraction.WebCore.Server.Interfaces;

public interface IProfileService
{
    public Task<ProfileDto?> GetUser(string identity);
    public Task<List<ProfileDto>> GetUsers();
    public Task<ControllerEnums.ProfileReturnState> CreateOrUpdate(ProfileDto request);
}
