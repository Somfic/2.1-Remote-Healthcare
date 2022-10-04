using System;
using System.Threading;
using System.Windows;
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

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Button pressed");
        }
    }
}