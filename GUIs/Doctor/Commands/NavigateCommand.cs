using System.Threading.Tasks;
using MvvmHelpers;
using RemoteHealthcare.GUIs.Doctor.Commands;

namespace RemoteHealthcare.GUIs.Doctor;

public class NavigateCommand : BaseCommand
{
    private readonly NavigationStore _navigationStore;
    private readonly ObservableObject _viewModel;

    public NavigateCommand(ObservableObject viewmodel, NavigationStore navigationStore)
    {
        _viewModel = viewmodel;
        _navigationStore = navigationStore;
    }

    public override void Execute(object? parameter)
    {
        ExecuteAsync();
    }

    public override async Task ExecuteAsync()
    {
        _navigationStore.CurrentViewModel = _viewModel;
    }
}