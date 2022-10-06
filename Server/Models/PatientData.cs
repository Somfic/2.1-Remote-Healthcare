using Newtonsoft.Json;

namespace RemoteHealthcare.Server.Models;

public class PatientData
{
    public List<Patient> Patients { get; set; }

    public PatientData()
    {
        Patients = new List<Patient>();
    }
    
    public static List<Patient> readUsersFromJson()
    {
        string path = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, "AllUsers.json");
        
        string returnAllUsersFromText = File.ReadAllText(path);
        
        List<Patient> data = JsonConvert.DeserializeObject<List<Patient>>(returnAllUsersFromText);
        
        return data;
    }

    public bool MatchLoginData(Patient patient)
    {
        Patients = readUsersFromJson();

        if (Patients.Exists(name => name.Password == patient.Password && name.UserId == patient.UserId))
        {
            return true;
        }
        
        return false;
    }

    public void SavePatientData()
    {
        var folderName = Environment.CurrentDirectory;
        Console.WriteLine(folderName);
        folderName = Path.Combine(folderName.Substring(0, folderName.LastIndexOf("bin")) + "PatientDataFiles");
        foreach (var patient in Patients)
        {
            patient.SaveSessionData(folderName);
        }
    }
}