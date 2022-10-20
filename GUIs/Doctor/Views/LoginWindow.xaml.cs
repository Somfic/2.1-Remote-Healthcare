using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using RemoteHealthcare.Common.Logger;

namespace RemoteHealthcare.GUIs.Doctor
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : UserControl
    {
        private readonly Log _log = new(typeof(LoginWindow));
        public LoginWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// If the DataContext is not null, then set the SecurePassword property of the DataContext to the SecurePassword
        /// property of the PasswordBox
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="RoutedEventArgs">This is the event arguments that are passed to the event handler.</param>
        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            {
                ((dynamic)this.DataContext).SecurePassword = ((PasswordBox)sender).SecurePassword;
            }
        }
    }
}