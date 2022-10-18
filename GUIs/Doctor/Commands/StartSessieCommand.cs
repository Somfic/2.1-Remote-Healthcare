using System;
using System.Threading.Tasks;
using RemoteHealthcare.Common;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor.Commands;

public class StartSessieCommand : BaseCommand
{
    private Client _client;
    private DoctorViewModel _viewModel;
    
    public StartSessieCommand()
    {
        
    }

    public override void Execute(object? parameter)
    {
        Console.WriteLine("Excuste van uit de cotor");   
        _client._client.SendAsync(new DataPacket<SessionStartPacketRequest>
        {
            OpperationCode = OperationCodes.SESSION_START,

            data = new SessionStartPacketRequest()
            {
                userId = 06111
            }
        });
    }

    public override async Task ExecuteAsync()
    {
        
    }
}