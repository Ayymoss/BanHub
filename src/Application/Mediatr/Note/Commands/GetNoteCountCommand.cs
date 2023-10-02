using MediatR;

namespace BanHub.Application.Mediatr.Note.Commands;

public class GetNoteCountCommand : IRequest<int>
{
    public string Identity { get; set; }

}
