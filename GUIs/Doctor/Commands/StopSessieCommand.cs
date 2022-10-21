using System;
using System.Threading.Tasks;
using RemoteHealthcare.Common;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor.Commands;

public class StopSessieCommand : BaseCommand
{
    private Client _client;
    private DoctorViewModel _viewModel;
    
    public StopSessieCommand(Client client)
    {
        _client = client;
    }

    public override void Execute(object? parameter)
    {
        Console.WriteLine("Stop van uit de doctor");   
        _client._client.SendAsync(new DataPacket<SessionStartPacketRequest>
        {
            OpperationCode = OperationCodes.SESSION_STOP,
        });
    }

    public override async Task ExecuteAsync()
    {
        
    }
}