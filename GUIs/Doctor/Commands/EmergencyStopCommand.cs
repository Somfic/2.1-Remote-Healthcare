using System;
using System.Threading.Tasks;
using System.Windows;
using MvvmHelpers;
using MvvmHelpers.Interfaces;

namespace RemoteHealthcare.GUIs.Doctor.Commands;

public class EmergencyStopCommand : BaseCommand
{
    public EmergencyStopCommand()
    {
        Console.WriteLine("Nieuw Branch");
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