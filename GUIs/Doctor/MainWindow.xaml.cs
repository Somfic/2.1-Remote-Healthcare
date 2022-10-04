﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RemoteHealthcare.Client;
using RemoteHealthcare.Common.Logger;

namespace Doctor
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
                    var client = new Client();
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