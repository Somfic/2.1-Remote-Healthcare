using System;
using System.Threading.Tasks;
using RemoteHealthcare.Common;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor.Commands;

public class StartSessieCommand : BaseCommand
{
    private Client _client;
    private DoctorViewModel _viewModel;
    
    public StartSessieCommand(Client client, DoctorViewModel viewModel)
    {
        _client = client;
        _viewModel = viewModel;
    }

    public override void Execute(object? parameter)
    {

        _client._client.SendAsync(new DataPacket<SessionStartPacketRequest>
        {
            OpperationCode = OperationCodes.SESSION_START,
            data = new SessionStartPacketRequest
            {
                selectedPatient = _viewModel.CurrentUser.UserId
            }
        });
    }

    public override Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}