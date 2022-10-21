using System;
using System.Threading.Tasks;
using RemoteHealthcare.Common;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor.Commands;

public class StartSessieCommand : BaseCommand
{
    private Client _client;
    private DoctorViewModel _viewModel;
    
    public StartSessieCommand(Client client)
    {
        _client = client;
    }

    public override void Execute(object? parameter)
    {
        Console.WriteLine("Execte van uit de Doctor");
        
        _client._client.SendAsync(new DataPacket<SessionStartPacketRequest>
        {
            OpperationCode = OperationCodes.SESSION_START,
            
        });
    }

    public override async Task ExecuteAsync()
    {
        
    }
}