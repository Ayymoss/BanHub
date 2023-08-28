using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Shared.Mediatr.Commands.PlayerProfile;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace BanHub.WebCore.Client.Components.Dialogs;

partial class ProfileAddNoteDialog
{
    [Parameter] public required string Identity { get; set; }

    [Inject] protected NoteService NoteService { get; set; }
    [Inject] protected NotificationService NotificationService { get; set; }
    [Inject] protected DialogService DialogService { get; set; }

    private bool _isPrivateNote = true;
    private string _noteMessage = string.Empty;
    private bool _processing;

    private async Task CreateNote()
    {
        _processing = true;

        if (_noteMessage.Length < 3)
        {
            NotificationService.Notify(NotificationSeverity.Warning, "Warning", "Too short of a note!");
            _processing = false;
            return;
        }

        var newNote = new AddNoteCommand
        {
            Message = _noteMessage,
            IsPrivate = _isPrivateNote,
            TargetIdentity = Identity,
        };
        var request = await NoteService.AddNoteAsync(newNote);

        if (!request)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to add note!");
            _processing = false;
            DialogService.Close();
            return;
        }

        NotificationService.Notify(NotificationSeverity.Success, "Success", "Note added!");
        _processing = false;
        DialogService.Close(newNote);
    }
}
