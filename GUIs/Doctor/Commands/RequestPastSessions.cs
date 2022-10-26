using System.Threading.Tasks;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor.Commands;

public class RequestPastSessions : BaseCommand
{
    private Log _log = new(typeof(RequestPastSessions));
    private Client _client;
    private DoctorViewModel _viewModel;

    public RequestPastSessions(Client client, DoctorViewModel doctorViewModel)
    {
        _client = client;
        _viewModel = doctorViewModel;
    }

    public override void Execute(object? parameter)
    {
        ExecuteAsync();
    }

    public override async Task ExecuteAsync()
    {
        string userId = _viewModel.CurrentUser.UserId;
        if (userId != null)
        {
            _client.HasSessionResponce = false;

            _client.Client.SendAsync(new DataPacket<AllSessionsFromPatientRequest>
            {
                OpperationCode = OperationCodes.GetPatientSesssions,
                Data = new AllSessionsFromPatientRequest
                {
                    UserId = userId
                }
            });

            while (!_client.HasSessionResponce)
            {
            }

            PastSessionsWindow pastSessionsWindow = new()
            {
                DataContext = new PastSessionsViewModel(_client, _viewModel.CurrentUser.Username)
            };
            pastSessionsWindow.Show();
        }
    }
}