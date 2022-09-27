namespace RemoteHealthcare.CentralServer;

public class Patientlist
{
    private Dictionary<string, string> users;




    public Patientlist()
    {
        users = new Dictionary<string, string>();
        users.Add("midas","1234");
        users.Add("nick","4321");
        users.Add("lucas","54321");
    
    }

    public bool Login(string login, string pass)
    {
        return (users.ContainsKey(login) && users[login] == pass);
    }
}