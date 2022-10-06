using System.Threading;
using RemoteHealthcare.Client;
using RemoteHealthcare.Common;
using System.Windows;
using System.Windows.Controls;

namespace Patient;

public partial class LoginWindow : Window
{
    private string _username;
    private string _password;
   
    public LoginWindow()
    {
        InitializeComponent();
        
        
        
       
    }

   

     

    private void UserName_Input(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (e.Source is TextBox)
        {
            _username = ((TextBox)e.Source).Text;
        }
    }

    private void Password_Input(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (e.Source is TextBox)
        {
            _password = ((TextBox)e.Source).Text;
        }
        
    }
    

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        //login button
        var client = new Client();
        client.RunAsync(_username,_password);
        Thread.Sleep(1000);
        if (!client.GetLoggedIn())
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }
        else
        {
            MessageBox.Show("Login failed");
        }
        


    }
    //if server response true than go to mainwindow
    private void GoToMainWindow()
    {
        MainWindow mainWindow = new MainWindow();
        mainWindow.Show();
        this.Close();
    }


}