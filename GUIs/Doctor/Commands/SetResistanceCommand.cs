using System.Threading.Tasks;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor.Commands;

public class SetResistanceCommand : BaseCommand
{
    private readonly Client _client;
    private readonly DoctorViewModel _viewModel;
    
    public SetResistanceCommand(Client client, DoctorViewModel doctorViewModel)
    {
        _client = client;
        _viewModel = doctorViewModel;
    }

    public override void Execute(object? parameter)
    {
        ExecuteAsync();
    }

    /// <summary>
    /// > The function sets the resistance of the current user to the resistance value in the view model
    /// </summary>
    public override async Task ExecuteAsync()
    {
        _client.SetResistance(_viewModel.CurrentUser.UserId, _viewModel.Resistance);
    }
}