using MediatR;

namespace BanHubData.Commands.Note;

public class GetNoteCountCommand : IRequest<int>
{
    public string Identity { get; set; }

}
