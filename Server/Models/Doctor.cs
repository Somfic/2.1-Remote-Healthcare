using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RemoteHealthcare.Server.Models;

public class Doctor
{
    
    public List<SessionData> Sessions { get; set; }
    internal string Username { get; set; }
    internal string UserId { get; set; }
    internal string Password { get; set; }

    public Doctor(string user, string pass, string userId)
    {
        this.Username = user;
        this.Password = pass;
        this.UserId = userId;
        this.Sessions = new List<SessionData>();
    }

    public void SaveSessionData(string foldername)
    {
        string pathString = Path.Combine(foldername, Username);
        Directory.CreateDirectory(pathString);
        foreach (var session in Sessions)
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