using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RemoteHealthcare.CentralServer.Models;

[Serializable]
public class Patient
{
    public List<SessionData> Sessions { get; set; }
    internal string Username { get; set; }
    internal string UserId { get; set; }
    internal string Password { get; set; }

    public Patient(string user, string pass, string userId)
    {
        Username = user;
        Password = pass;
        UserId = userId;
        Sessions = new List<SessionData>();
    }

    public void SaveSessionData(string folderName)
    {
        var pathString = Path.Combine(folderName, Username);
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