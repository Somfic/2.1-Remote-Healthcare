using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RemoteHealthcare.Server.Models;

[Serializable]
public class Patient
{
    public List<SessionData> Sessions { get; set; }
    public string UserId { get; set; }
    public string? Nickname { get; set; }
    public string Password { get; set; }

    public Patient(string user_id, string password)
    {
        UserId = user_id;
        Password = password;
        Nickname = "test";
        Sessions = new List<SessionData>();
    }

    /// <summary>
    /// It takes a folder name as a parameter, creates a directory with the user's username, and then creates a file for
    /// each session in the user's session list
    /// </summary>
    /// <param name="folderName">The name of the folder you want to save the data to.</param>
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