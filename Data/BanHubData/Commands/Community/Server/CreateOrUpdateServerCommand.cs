using BanHubData.Enums;
using MediatR;

namespace BanHubData.Commands.Community.Server;

public class CreateOrUpdateServerCommand : IRequest<ControllerEnums.ReturnState>
{
    public string ServerName { get; set; }
    public Game ServerGame { get; set; }
    public string ServerId { get; set; }
    public string ServerIp { get; set; }
    public int ServerPort { get; set; }
    public Guid CommunityGuid { get; set; }
}
