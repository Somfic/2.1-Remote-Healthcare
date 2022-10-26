using System.Threading.Tasks;
using RemoteHealthcare.Common;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor.Commands;

public class StopSessieCommand : BaseCommand
{
    private DoctorClient _doctorClient;
    private DoctorViewModel _viewModel;
    
    public StopSessieCommand(DoctorClient doctorClient, DoctorViewModel viewModel)
    {
        _doctorClient = doctorClient;
        _viewModel = viewModel;
    }

    public override void Execute(object? parameter)
    {
        _doctorClient.Client.SendAsync(new DataPacket<SessionStopPacketRequest>
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