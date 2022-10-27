using System.Threading.Tasks;
using MvvmHelpers;

namespace RemoteHealthcare.GUIs.Doctor.Commands;

public class NavigateCommand : BaseCommand
{
    private readonly ObservableObject _viewModel;
    private readonly NavigationStore _navigationStore;

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