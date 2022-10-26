using System.Threading.Tasks;
using RemoteHealthcare.Common;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor.Commands;

public class StopSessieCommand : BaseCommand
{
    private Client _client;
    private DoctorViewModel _viewModel;
    
    public StopSessieCommand(Client client, DoctorViewModel viewModel)
    {
        _client = client;
        _viewModel = viewModel;
    }

    public override void Execute(object? parameter)
    {
        _client.Client.SendAsync(new DataPacket<SessionStopPacketRequest>
        {
            OpperationCode = OperationCodes.SessionStop,
            Data = new SessionStopPacketRequest
            {
                SelectedPatient = _viewModel.CurrentUser.UserId
            }
        });
    }

    public override async Task ExecuteAsync()
    {
        
    }
}