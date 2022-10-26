using System.Threading.Tasks;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor.Commands;

public class SendChatMessageCommand : BaseCommand
{
    private readonly DoctorClient _doctorClient;
    private readonly DoctorViewModel _viewModel;

    public SendChatMessageCommand(DoctorClient doctorClient, DoctorViewModel doctorViewModel)
    {
        _doctorClient = doctorClient;
        _viewModel = doctorViewModel;
    }

    public override void Execute(object? parameter)
    {
        ExecuteAsync();
    }

    public override async Task ExecuteAsync()
    {
        _viewModel.ChatMessages.Add("U: " + _viewModel.TextBoxChatMessage);
        _doctorClient.SendChatAsync(_viewModel.CurrentUser.UserId, _viewModel.TextBoxChatMessage);
    }
}