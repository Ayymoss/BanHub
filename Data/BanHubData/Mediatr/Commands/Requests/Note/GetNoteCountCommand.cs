using MediatR;

namespace BanHubData.Mediatr.Commands.Requests.Note;

public class GetNoteCountCommand : IRequest<int>
{
    public string Identity { get; set; }

}
