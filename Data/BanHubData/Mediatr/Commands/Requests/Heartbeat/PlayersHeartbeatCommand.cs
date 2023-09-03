﻿using BanHubData.Enums;
using MediatR;

namespace BanHubData.Mediatr.Commands.Requests.Heartbeat;

public class PlayersHeartbeatCommand : IRequest<SignalREnums.ReturnState>
{
    public Version PluginVersion { get; set; }
    public Guid CommunityGuid { get; set; }
    public List<string> PlayerIdentities { get; set; }
}