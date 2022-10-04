using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using RemoteHealthcare.Common.Logger;

namespace RemoteHealthcare.GUIs.Doctor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Log _log = new(typeof(MainWindow));
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                new Thread(async () =>
                {
                    var client = new Client.Client();
                    _log.Debug("Client created");
                    await client.RunAsync();
                }).Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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