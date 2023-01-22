using GlobalInfraction.WebCore.Server.Services;
using GlobalInfraction.WebCore.Server.Services.Authentication;
using GlobalInfraction.WebCore.Shared.DTOs.WebEntity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlobalInfraction.WebCore.Server.Controllers;

[ApiController]
[Route("api/v2/[controller]")]
public class AuthController : ControllerBase
{
    private readonly WebAuthentication _webAuthentication;
    private readonly Configuration _configuration;

    public AuthController(WebAuthentication webAuthentication, Configuration configuration)
    {
        _webAuthentication = webAuthentication;
        _configuration = configuration;
    }

    [HttpPost("Login")]
    public async Task<ActionResult<UserDto>> Login([FromBody] LoginRequestDto loginRequest)
    {
        var authManager = new JwtAuthenticationManager(_configuration, _webAuthentication);
        var userSession = await authManager.GenerateJwtToken(loginRequest.Token ?? string.Empty);
        if (userSession is null) return Unauthorized();
        return Ok(userSession);
    }

    [HttpGet, Authorize(Roles = "Admin")]
    public ActionResult<string> GetCat() => Ok("Meow");
}
