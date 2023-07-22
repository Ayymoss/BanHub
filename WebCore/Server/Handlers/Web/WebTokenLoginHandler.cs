﻿using System.Security.Claims;
using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Commands;
using BanHub.WebCore.Shared.Commands.Web;
using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web;

public class WebTokenLoginHandler : IRequestHandler<WebTokenLoginCommand, WebTokenLoginCommandResponse>
{
    private readonly DataContext _context;
    private readonly SignedInUsers _signedInUsers;

    public WebTokenLoginHandler(DataContext context, SignedInUsers signedInUsers)
    {
        _context = context;
        _signedInUsers = signedInUsers;
    }

    public async Task<WebTokenLoginCommandResponse> Handle(WebTokenLoginCommand request, CancellationToken cancellationToken)
    {
        var token = await _context.AuthTokens
            .AsTracking()
            .Where(x => x.Token == request.Token)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (token is null || token.Expiration < DateTimeOffset.UtcNow || token.Used)
            return new WebTokenLoginCommandResponse
            {
                ReturnState = ControllerEnums.ReturnState.Unauthorized,
                ClaimsIdentity = null
            };

        var user = await _context.Players
            .Where(x => x.Id == token.PlayerId)
            .Select(x => new WebUser
            {
                UserName = x.CurrentAlias.Alias.UserName,
                WebRole = x.WebRole.ToString(),
                CommunityRole = x.CommunityRole.ToString(),
                Identity = x.Identity,
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);


        if (user is null)
            return new WebTokenLoginCommandResponse
            {
                ReturnState = ControllerEnums.ReturnState.Unauthorized,
                ClaimsIdentity = null
            };

        user.SignedInGuid = Guid.NewGuid().ToString();
        token.Used = true;
        _context.AuthTokens.Update(token);
        await _context.SaveChangesAsync(cancellationToken);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, $"Web_{user.WebRole}"),
            new(ClaimTypes.Role, $"Community_{user.CommunityRole}"),
            new(ClaimTypes.NameIdentifier, user.Identity),
            new("SignedInGuid", user.SignedInGuid),
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        _signedInUsers.AddUser(user);
        return new WebTokenLoginCommandResponse
        {
            ReturnState = ControllerEnums.ReturnState.Ok,
            ClaimsIdentity = claimsIdentity
        };
    }
}
