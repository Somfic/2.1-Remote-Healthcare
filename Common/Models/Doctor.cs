using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RemoteHealthcare.Server.Models;

public class Doctor
{
    public Doctor(string user, string pass, string userId)
    {
        Username = user;
        Password = pass;
        UserId = userId;
        Sessions = new List<SessionData>();
    }

    public List<SessionData> Sessions { get; set; }
    public string Username { get; set; }
    public string UserId { get; set; }
    public string Password { get; set; }

    public void SaveSessionData(string foldername)
    {
        var pathString = Path.Combine(foldername, Username);
        Directory.CreateDirectory(pathString);
        foreach (var session in Sessions)
        {
            var filename = session.Id;
            var json = JsonConvert.SerializeObject(session);
            pathString = Path.Combine(pathString, filename);

            if (!File.Exists(pathString))
            {
                File.WriteAllText(pathString, JObject.Parse(json).ToString());
            }
        }
    }
}