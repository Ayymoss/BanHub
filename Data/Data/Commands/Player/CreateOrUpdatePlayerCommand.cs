using Data.Enums;
using MediatR;

namespace Data.Commands.Player;

public class CreateOrUpdatePlayerCommand : IRequest<string>
{
    public string PlayerIdentity { get; set; }
    public string PlayerAliasUserName { get; set; }
    public string PlayerAliasIpAddress { get; set; }
    public InstanceRole PlayerInstanceRole { get; set; }
    public Guid InstanceGuid { get; set; }
    public string? ServerId { get; set; }
}
