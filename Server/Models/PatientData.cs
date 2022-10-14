using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteHealthcare.Common.Logger;

namespace RemoteHealthcare.Server.Models;

public class PatientData
{
    private readonly Log _log = new Log(typeof(PatientData));
    
    public List<Patient> Patients { get; set; }

    public PatientData()
    {
        Patients = new List<Patient>
        {
            new("Johan Talboom", "1234", "3245"),
            new("Hans Van der linden", "1234", "3245"),
            new("Co Nelen", "1234", "3245")
        };
    }
    
    public static List<Patient> readUsersFromJson()
    {
        string path = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, "AllUsers.json");
        
        string returnAllUsersFromText = File.ReadAllText(path);
        
        List<Patient> data = JsonConvert.DeserializeObject<List<Patient>>(returnAllUsersFromText);
        
        return data;
    }

    /// <summary>
    /// This function checks if the username, password and userid of the patient object passed in as a parameter matches the
    /// username, password and userid of any of the patients in the list of patients
    /// </summary>
    /// <param name="Patient">The patient object that is being passed in.</param>
    /// <returns>
    /// A boolean value.
    /// </returns>
    public bool MatchLoginData(Patient patient)
    {
        Patients = readUsersFromJson();
        
        //Checks if the Patient parameter exists in the AllUsers.json with LINQ
        if (Patients.Exists(name => name.Password == patient.Password && name.UserId == patient.UserId))
        {
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// It takes the current directory, removes the last instance of "bin" from it, and then adds "PatientDataFiles" to the
    /// end of it
    /// </summary>
    public void SavePatientData()
    {
        var folderName = Environment.CurrentDirectory;
        _log.Debug(folderName);
        folderName = Path.Combine(folderName.Substring(0, folderName.LastIndexOf("bin")) + "PatientDataFiles");
        foreach (var patient in Patients)
        {
            patient.SaveSessionData(folderName);
        }
    }

    public JObject[] GetPatientDataAsJObjects()
    {
        JObject[] jObjects = new JObject[Patients.Count];
        
        for (int i = 0; i < Patients.Count; i++)
        {
            jObjects[i] = Patients[i].GetPatientAsJObject();
        }

        return jObjects;
    }
}