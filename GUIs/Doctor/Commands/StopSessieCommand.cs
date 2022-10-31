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

    /// <summary>
    /// The function sends a packet to the server to stop the session
    /// </summary>
    /// <param name="parameter">The parameter passed to the command.</param>
    public override void Execute(object? parameter)
    {
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