﻿using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Commands.Chat;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Chat;

public class GetMessageContextHandler : IRequestHandler<GetMessageContextCommand, ChatContextRoot>
{
    private readonly DataContext _context;

    public GetMessageContextHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<ChatContextRoot> Handle(GetMessageContextCommand request, CancellationToken cancellationToken)
    {
        var chats = await _context.Chats
            .Where(x => x.Server.ServerId == request.ServerId)
            .Where(x => x.Submitted >= request.Submitted.AddMinutes(-5))
            .Where(x => x.Submitted < request.Submitted.AddMinutes(5))
            .Select(x => new ChatContext
            {
                Submitted = x.Submitted,
                Message = x.Message,
                PlayerUserName = x.Player.CurrentAlias.Alias.UserName
            }).ToListAsync(cancellationToken: cancellationToken);

        var result = new ChatContextRoot
        {
            Messages = chats.OrderBy(x => x.Submitted),
            Loaded = true
        };
        return result;
    }
}