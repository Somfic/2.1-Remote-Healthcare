using System;
using System.Threading.Tasks;
using System.Windows;
using MvvmHelpers;
using MvvmHelpers.Interfaces;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor.Commands;

public class EmergencyStopCommand : BaseCommand
{
    private readonly NavigationStore _navigationStore;
    private readonly LoginWindowViewModel _loginWindowViewModel;
    
    public EmergencyStopCommand(Client client, DoctorViewModel doctorViewModel)
    {
    }

    public override void Execute(object? parameter)
    {
        MessageBox.Show("Emergency stop was pressed, Please check on the patient before continuing.");
    }

    public override Task ExecuteAsync()
    {
        throw new System.NotImplementedException();
    }
}