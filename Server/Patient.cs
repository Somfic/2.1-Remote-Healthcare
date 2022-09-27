namespace RemoteHealthcare.CentralServer;
[Serializable]
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

    public void SaveSessionData(string foldername)
    {
        string pathString = Path.Combine(foldername, username);
        Directory.CreateDirectory();
        foreach (var session in _sessions)
        {
            string filename = session.Id;
            
        }
    }
}