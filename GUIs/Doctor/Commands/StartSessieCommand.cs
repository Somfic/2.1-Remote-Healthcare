using System;
using System.Threading.Tasks;
using RemoteHealthcare.Common;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor.Commands;

public class StartSessieCommand : BaseCommand
{
    private readonly Client _client;
    private readonly DoctorViewModel _viewModel;
    
    public StartSessieCommand(Client client, DoctorViewModel viewModel)
    {
        _client = client;
        _viewModel = viewModel;
    }

    /// <summary>
    /// The function sends a packet to the server to start a session with the selected patient
    /// </summary>
    /// <param name="parameter">The parameter passed to the command.</param>
    public override void Execute(object? parameter)
    {

        _client._client.SendAsync(new DataPacket<SessionStartPacketRequest>
        {
            OpperationCode = OperationCodes.SessionStart,
            data = new SessionStartPacketRequest()
            {
                SelectedPatient = _viewModel.CurrentUser.UserId
            }
        });
    }

    public override Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}