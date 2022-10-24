using System.Runtime.InteropServices;
using MvvmHelpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteHealthcare.Common.Logger;

namespace RemoteHealthcare.Server.Models;

[Serializable]
public class Patient : ObservableObject
{
    private Log _log = new Log(typeof(Patient));
    public List<SessionData> Sessions { get; set; }

    public string Username { get; set; }
    public string UserId { get; set; }
    public string Password { get; set; }

    public Patient(string user, string password, string? username = null)
    {
        Password = password;
        UserId = user;
        if (username != null)
            Username = username;
        Sessions = new List<SessionData>();
    }

    /// <summary>
    /// It takes a folder name as a parameter, creates a directory with the user's username, and then creates a file for
    /// each session in the user's session list
    /// </summary>
    /// <param name="pathString">The name of the folder you want to save the data to.</param>
    public void SaveSessionData(string pathString)
    {
        pathString = Path.Combine(pathString.Substring(0, pathString.LastIndexOf("bin")));

        pathString = Path.Combine(pathString, "allSessions");


        if (!Directory.Exists(pathString))
            Directory.CreateDirectory(pathString);

        foreach (var session in Sessions)
        {
            var pathStringUserId = Path.Combine(pathString, UserId);

            if (!Directory.Exists(pathStringUserId))
                Directory.CreateDirectory(pathStringUserId);

            var filename = session.SessionId.Replace(':', '-') + "-" + session.Id;
            var json = JsonConvert.SerializeObject(session);
            var pathStringFileName = Path.Combine(pathStringUserId, filename + ".json");

            File.WriteAllText(pathStringFileName, JObject.Parse(json).ToString());
        _log.Debug($"Saved to path: \r\n{pathStringFileName}");
        }
    }

    public override string ToString()
    {
        return $" Patient username: {Username}; UserId {UserId}";
    }

    public JObject GetPatientAsJObject()
    {
        return JObject.Parse(JsonConvert.SerializeObject(this));
    }
}