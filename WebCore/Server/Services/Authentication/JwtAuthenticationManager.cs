using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GlobalInfraction.WebCore.Server.Context;
using GlobalInfraction.WebCore.Shared.DTOs.WebEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.IdentityModel.Tokens;

namespace GlobalInfraction.WebCore.Server.Services.Authentication;

public class JwtAuthenticationManager
{
    private readonly WebAuthentication _webAuthentication;

    private readonly Configuration _configuration;

    private const int JwtTokenValidityMinutes = 60;

    public JwtAuthenticationManager(Configuration configuration, WebAuthentication webAuthentication)
    {
        _configuration = configuration;
        _webAuthentication = webAuthentication;
    }

    public async Task<UserDto?> GenerateJwtToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;

        var user = await _webAuthentication.GetAccountByToken(token);
        if (user == null) return null;

        var tokenExpirationTime = DateTime.UtcNow.AddMinutes(JwtTokenValidityMinutes);
        var tokenKey = Encoding.ASCII.GetBytes(_configuration.JwtSecurityKey);
        var claimsIdentity = new ClaimsIdentity(new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, user.Role),
        });
        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature);
        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Expires = tokenExpirationTime,
            SigningCredentials = signingCredentials
        };
        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        var securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
        var tokenString = jwtSecurityTokenHandler.WriteToken(securityToken);

        var userDto = new UserDto
        {
            UserName = user.UserName,
            Role = user.Role,
            Token = tokenString,
            ExpiresIn = JwtTokenValidityMinutes,
        };
        return userDto;
    }
}
