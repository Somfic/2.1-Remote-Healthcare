using System.Threading.Tasks;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor.Commands;

public class SendChatMessageCommand : BaseCommand
{
    private Client _client;
    private DoctorViewModel _viewModel;
    
    public SendChatMessageCommand(Client client, DoctorViewModel doctorViewModel)
    {
        _client = client;
        _viewModel = doctorViewModel;
    }

    public override void Execute(object? parameter)
    {
        ExecuteAsync();
    }

    public override async Task ExecuteAsync()
    {
        _viewModel.ChatMessages.Add(_viewModel.TextBoxChatMessage);
        _client.SendChatAsync(_viewModel.CurrentUser.UserId, _viewModel.TextBoxChatMessage);
    }
}