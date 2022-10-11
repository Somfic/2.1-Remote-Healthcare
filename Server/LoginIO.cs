using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RemoteHealthcare.Server.Models;

namespace Server
{
    public class LoginIO
    {
        public static List<Patient> AllUsersInList = new List<Patient>();
        public string path = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, "AllUsers.json");
        


        public static Patient readUsersFromJson()
        {
            string path = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, "AllUsers.json"); ;

            string returnAllUsersFromText = File.ReadAllText(path);

            Patient data = JsonConvert.DeserializeObject<Patient>(returnAllUsersFromText);

            return data;
        }
    }
}

