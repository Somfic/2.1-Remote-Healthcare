using System;
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
        Console.WriteLine("Stop van uit de doctor");   
        
        _client._client.SendAsync(new DataPacket<SessionStopPacketRequest>
        {
            OpperationCode = OperationCodes.SESSION_STOP,
            data = new SessionStopPacketRequest()
            {
                selectedPatient = _viewModel.CurrentUser.UserId
            }
        });
    }

    public override async Task ExecuteAsync()
    {
        
    }
}