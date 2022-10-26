using System.Threading.Tasks;
using RemoteHealthcare.Common;
using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor.Commands;

public class RequestPastSessions : BaseCommand
{
    private readonly DoctorClient _doctorClient;
    private Log _log = new(typeof(RequestPastSessions));
    private readonly DoctorViewModel _viewModel;

    public RequestPastSessions(DoctorClient doctorClient, DoctorViewModel doctorViewModel)
    {
        _doctorClient = doctorClient;
        _viewModel = doctorViewModel;
    }

    public override void Execute(object? parameter)
    {
        ExecuteAsync();
    }

    public override async Task ExecuteAsync()
    {
        var userId = _viewModel.CurrentUser.UserId;
        if (userId != null)
        {
            _doctorClient.HasSessionResponce = false;

            _doctorClient.Client.SendAsync(new DataPacket<AllSessionsFromPatientRequest>
            {
                OpperationCode = OperationCodes.GetPatientSesssions,
                Data = new AllSessionsFromPatientRequest
                {
                    UserId = userId
                }
            });

            while (!_doctorClient.HasSessionResponce)
            {
            }

            PastSessionsWindow pastSessionsWindow = new()
            {
                DataContext = new PastSessionsViewModel(_doctorClient, _viewModel.CurrentUser.Username)
            };
            pastSessionsWindow.Show();
        }
    }
}