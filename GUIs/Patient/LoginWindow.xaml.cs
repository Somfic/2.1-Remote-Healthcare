using System.Windows;
using System.Windows.Controls;

namespace RemoteHealthcare.GUIs.Patient
{

    public partial class LoginWindow : Window
    {

        public LoginWindow()
        {
            InitializeComponent();

        }
       
        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            {
                ((dynamic)this.DataContext).SecurePassword = ((PasswordBox)sender).SecurePassword;
            }
        }

    }
}