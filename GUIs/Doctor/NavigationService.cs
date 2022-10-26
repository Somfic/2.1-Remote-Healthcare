using System;
using MvvmHelpers;

namespace RemoteHealthcare.GUIs.Doctor;

public class NavigationService<TObservableObject>
    where TObservableObject : ObservableObject
{
    private readonly Func<TObservableObject> _createViewModel;
    private readonly NavigationStore _navigationStore;

    public NavigationService(NavigationStore navigationStore, Func<TObservableObject> createViewModel)
    {
        _navigationStore = navigationStore;
        _createViewModel = createViewModel;
    }

    public void Navigate()
    {
        _navigationStore.CurrentViewModel = _createViewModel();
    }
}