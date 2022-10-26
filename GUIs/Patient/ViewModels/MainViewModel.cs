namespace RemoteHealthcare.GUIs.Patient.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly NavigationStore _navigationStore;

    public MainViewModel(NavigationStore navigationStore)
    {
        _navigationStore = navigationStore;
        _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
    }

    public BaseViewModel CurrentViewModel => _navigationStore.CurrentViewModel;

    private void OnCurrentViewModelChanged()
    {
        OnPropertyChanged(nameof(CurrentViewModel));
    }
}