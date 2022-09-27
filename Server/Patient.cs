namespace RemoteHealthcare.CentralServer;

public class Patient
{
    private List<SessionData> _sessions { get; set; }
    private string username { get; set; }
    private string password { get; set; }

    public Patient(string user, string pass)
    {
        this.username = user;
        this.password = pass;
        _sessions = new List<SessionData>();
    }
}