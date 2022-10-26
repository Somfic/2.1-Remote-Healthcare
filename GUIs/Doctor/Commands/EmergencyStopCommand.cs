using System.Threading.Tasks;
using System.Windows;
using RemoteHealthcare.Common;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor.Commands;

public class EmergencyStopCommand : BaseCommand
{
    private DoctorClient _doctorClient;
    private DoctorViewModel _viewModel;
    
    public EmergencyStopCommand(DoctorClient doctorClient, DoctorViewModel doctorViewModel)
    {
        _doctorClient = doctorClient;
        _viewModel = doctorViewModel;
    }

    public override void Execute(object? parameter)
    {
        _doctorClient.Client.SendAsync(new DataPacket<EmergencyStopPacket>
        {
            OpperationCode = OperationCodes.EmergencyStop,
            Data = new EmergencyStopPacket
            {
                StatusCode = StatusCodes.Ok,
                ClientId = _viewModel.CurrentUser.UserId,
            }
        });
        MessageBox.Show("Emergency stop was pressed, Please check on the patient before continuing.");
    }

    public override async Task ExecuteAsync()
    {
        
    }
}