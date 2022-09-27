using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RemoteHealthcare.CentralServer;
[Serializable]
public class Patient
{
    public List<SessionData> _sessions { get; set; }
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
        Directory.CreateDirectory(pathString);
        foreach (var session in _sessions)
        {
            string filename = session.Id;
            string json = JsonConvert.SerializeObject(session);
            pathString = Path.Combine(pathString, filename);

            if (!File.Exists(pathString))
            {
                File.WriteAllText(pathString, JObject.Parse(json).ToString());
            }
        }
    }
}