using System.Windows;
using System.Windows.Controls;

namespace RemoteHealthcare.GUIs.Patient.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext != null) ((dynamic)DataContext).SecurePassword = ((PasswordBox)sender).SecurePassword;
    }
}