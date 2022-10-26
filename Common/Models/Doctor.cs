using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteHealthcare.Common.Cryptography;

namespace RemoteHealthcare.Server.Models;

public class Doctor
{
    
    public List<SessionData> _sessions { get; set; }
    public string Username { get; set; }
    public string UserId { get; set; }
    public string Password { get; set; }

    public Doctor(string user, string pass, string UserId)
    {
        this.Username = user;
        this.Password = pass;
        this.UserId = UserId;
        this._sessions = new List<SessionData>();
    }

    public void SaveSessionData(string foldername)
    {
        string pathString = Path.Combine(foldername, Username);
        Directory.CreateDirectory(pathString);
        foreach (var session in _sessions)
        {
            string filename = session.Id;
            string json = JsonConvert.SerializeObject(session);
            pathString = Path.Combine(pathString, filename);

            string content = JObject.Parse(json).ToString();
            var encrypted = Encryption.Encrypt(content, Password);

            if (!File.Exists(pathString))
            {
                File.WriteAllText(pathString, encrypted);
            }
        }
    }
}