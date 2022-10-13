using System.Windows;
using System.Windows.Controls;

namespace RemoteHealthcare.GUIs.Patient.Views
{

    public partial class LoginView : UserControl
    {

        public LoginView()
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