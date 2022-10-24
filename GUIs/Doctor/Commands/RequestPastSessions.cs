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
            _log.Debug($"Execute has been entered for id: {userId}");

            _client._client.SendAsync(new DataPacket<AllSessionsFromPatientRequest>
            {
                OpperationCode = OperationCodes.GET_PATIENT_SESSSIONS,
                data = new AllSessionsFromPatientRequest()
                {
                    userId = userId
                }
            });
        }
    }
}