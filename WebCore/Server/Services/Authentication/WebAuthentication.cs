using GlobalInfraction.WebCore.Server.Context;
using GlobalInfraction.WebCore.Server.Models;
using GlobalInfraction.WebCore.Shared.DTOs.WebEntity;
using Microsoft.EntityFrameworkCore;

namespace GlobalInfraction.WebCore.Server.Services.Authentication;

public class WebAuthentication
{
    private readonly DataContext _context;

    public WebAuthentication(DataContext context)
    {
        _context = context;
    }

    public async Task<UserDto?> GetAccountByToken(string token)
    {
        var tokenObject = await _context.AuthTokens.AsTracking().FirstOrDefaultAsync(x => x.Token == token && x.Active);
        if (tokenObject is null) return null;
        var user = await _context.Entities.Where(x => x.Id == tokenObject.EntityId).Select(x => new UserDto
        {
            UserName = x.CurrentAlias.Alias.UserName,
            Role = x.WebRole.ToString(),
        }).FirstOrDefaultAsync();

        tokenObject.Active = false;
        _context.AuthTokens.Update(tokenObject);
        await _context.SaveChangesAsync();
        return user;
    }
}
