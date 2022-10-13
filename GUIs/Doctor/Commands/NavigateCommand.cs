using System;
using System.Threading.Tasks;
using MvvmHelpers.Commands;
using RemoteHealthcare.GUIs.Doctor.Commands;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor;

public class NavigateCommand : BaseCommand
{
    private readonly NavigationStore _navigationStore;

    public NavigateCommand(NavigationStore navigationStore)
    {
        _navigationStore = navigationStore;
    }

    public override void Execute(object? parameter)
    {
        throw new NotImplementedException();
    }

    public override Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}