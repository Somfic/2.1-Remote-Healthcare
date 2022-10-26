using System;
using System.Threading.Tasks;
using System.Windows;
using MvvmHelpers;
using MvvmHelpers.Interfaces;
using RemoteHealthcare.Common;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor.Commands;

public class EmergencyStopCommand : BaseCommand
{
    private Client _client;
    private DoctorViewModel _viewModel;
    
    public EmergencyStopCommand(Client client, DoctorViewModel doctorViewModel)
    {
        _client = client;
        _viewModel = doctorViewModel;
    }

    /// <summary>
    /// This function sends a packet to the server to tell it that the emergency stop button was pressed
    /// </summary>
    /// <param name="parameter">This is the parameter that is passed in from the view.</param>
    public override void Execute(object? parameter)
    {
        _client._client.SendAsync(new DataPacket<EmergencyStopPacket>
        {
            OpperationCode = OperationCodes.EMERGENCY_STOP,
            data = new EmergencyStopPacket()
            {
                statusCode = StatusCodes.OK,
                clientId = _viewModel.CurrentUser.UserId,
            }
        });
        MessageBox.Show("Emergency stop was pressed, Please check on the patient before continuing.");
    }

    public override async Task ExecuteAsync()
    {
    }
}