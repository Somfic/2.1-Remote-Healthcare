using System;
using MvvmHelpers;

namespace RemoteHealthcare.GUIs.Doctor;

public class NavigationService<TObservableObject>
    where TObservableObject : ObservableObject
{
    private readonly NavigationStore _navigationStore;

    private readonly Func<TObservableObject> _createViewModel;

    public NavigationService(NavigationStore navigationStore, Func<TObservableObject> createViewModel)
    {
        _navigationStore = navigationStore;
        _createViewModel = createViewModel;
    }

    public void Navigate()
    {
        Console.WriteLine("Navigating to new Viewmodel");
        _navigationStore.CurrentViewModel = _createViewModel();
        Console.WriteLine("Navigated to new Viewmodel");
    }
}