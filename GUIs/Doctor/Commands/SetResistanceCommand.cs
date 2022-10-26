using System.Threading.Tasks;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor.Commands;

public class SetResistanceCommand : BaseCommand
{
    private DoctorClient _doctorClient;
    private DoctorViewModel _viewModel;
    
    public SetResistanceCommand(DoctorClient doctorClient, DoctorViewModel doctorViewModel)
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
        _doctorClient.SetResistance(_viewModel.CurrentUser.UserId, _viewModel.Resistance);
    }
}