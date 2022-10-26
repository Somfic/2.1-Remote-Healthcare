using System;
using System.Threading.Tasks;
using RemoteHealthcare.Common;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor.Commands;

public class StartSessieCommand : BaseCommand
{
    private readonly DoctorClient _doctorClient;
    private readonly DoctorViewModel _viewModel;

    public StartSessieCommand(DoctorClient doctorClient, DoctorViewModel viewModel)
    {
        _doctorClient = doctorClient;
        _viewModel = viewModel;
    }

    public override void Execute(object? parameter)
    {
        _doctorClient.Client.SendAsync(new DataPacket<SessionStartPacketRequest>
        {
            OpperationCode = OperationCodes.SessionStart,
            Data = new SessionStartPacketRequest
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