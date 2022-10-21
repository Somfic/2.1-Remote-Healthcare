using MvvmHelpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteHealthcare.Common.Logger;

namespace RemoteHealthcare.Server.Models;

[Serializable]
public class Patient
{
    private Log _log = new Log(typeof(Patient));
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
        var pathString = Path.Combine(folderName, Nickname);
        Directory.CreateDirectory(pathString);
        
        foreach (var session in Sessions)
        {
            pathString = Path.Combine(folderName, UserId);
            var filename = session.SessionId.Replace(':','-') + "-" + session.Id;
            var json = JsonConvert.SerializeObject(session);
            pathString = Path.Combine(pathString, filename);

            if (!File.Exists(pathString))
            {
                File.WriteAllText(pathString, JObject.Parse(json).ToString());
            } else
            {
                File.WriteAllText(pathString, JObject.Parse(json).ToString());
            }
        }
    }

    public override string ToString()
    {
        return $" Patient: {Nickname} + UserId {UserId}";
    }

    public JObject GetPatientAsJObject()
    {
        return JObject.Parse(JsonConvert.SerializeObject(this));
    }
}