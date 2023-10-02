using MediatR;

namespace BanHub.Application.Mediatr.Player.Commands;

public class AddNoteCommand : IRequest<bool>
{
    public string Message { get; set; }
    public bool IsPrivate { get; set; }
    public string TargetIdentity { get; set; }
    public string? AdminIdentity { get; set; }
}
