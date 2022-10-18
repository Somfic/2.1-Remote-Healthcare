using Newtonsoft.Json;
using RemoteHealthcare.Server.Models;

namespace Server
{
    public class LoginIo
    {
        public static List<Patient> AllUsersInList = new List<Patient>();
        public string Path = System.IO.Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, "AllUsers.json");
        


        public static Patient ReadUsersFromJson()
        {
            string path = System.IO.Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, "AllUsers.json"); ;

            string returnAllUsersFromText = File.ReadAllText(path);

            Patient data = JsonConvert.DeserializeObject<Patient>(returnAllUsersFromText);

            return data;
        }
    }
}

