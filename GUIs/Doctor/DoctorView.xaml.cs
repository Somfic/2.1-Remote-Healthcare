using System.Windows;
using RemoteHealthcare.GUIs.Doctor.ViewModels;

namespace RemoteHealthcare.GUIs.Doctor;

public partial class DoctorView : Window
{
    public DoctorView(Client.Client client)
    {
        InitializeComponent();
        ((DoctorViewModel)this.DataContext).DoctorClient = client;
    }
}