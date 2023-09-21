using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.PlayerProfile;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace BanHub.WebCore.Client.Components.Dialogs;

partial class ProfileAddNoteDialog(NoteService noteService, NotificationService notificationService, DialogService dialogService)
{
    [Parameter] public required string Identity { get; set; }

    private bool _isPrivateNote = true;
    private string _noteMessage = string.Empty;
    private bool _processing;

    private async Task CreateNote()
    {
        _processing = true;

        if (_noteMessage.Length < 3)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Warning", "Too short of a note!");
            _processing = false;
            return;
        }

        var newNote = new AddNoteCommand
        {
            Message = _noteMessage,
            IsPrivate = _isPrivateNote,
            TargetIdentity = Identity,
        };
        var request = await noteService.AddNoteAsync(newNote);

        if (!request)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", "Failed to add note!");
            _processing = false;
            dialogService.Close();
            return;
        }

        notificationService.Notify(NotificationSeverity.Success, "Success", "Note added!");
        _processing = false;
        dialogService.Close(newNote);
    }
}
