using System;
using System.Threading.Tasks;
using MvvmHelpers;
using MvvmHelpers.Commands;
using RemoteHealthcare.GUIs.Doctor.Commands;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor;

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

    public async override Task ExecuteAsync()
    {
        _navigationStore.CurrentViewModel = _viewModel;
    }
}