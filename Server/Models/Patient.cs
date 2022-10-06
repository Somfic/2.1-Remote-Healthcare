using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RemoteHealthcare.Server.Models;

[Serializable]
public class Patient
{
    public List<SessionData> Sessions { get; set; }
    internal string UserId { get; set; }
    internal string? Nickname { get; set; }
    internal string Password { get; set; }

    public Patient(string user_id, string password)
    {
        UserId = user_id;
        Password = password;
        Nickname = "test";
        Sessions = new List<SessionData>();
    }

    public void SaveSessionData(string folderName)
    {
        //TODO kijken hoe dit precies opgeslagen wordt.
        var pathString = Path.Combine(folderName, UserId);
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