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
        /*Patients = new List<Patient>
        {
            new("Johan Talboom", "1234", "3245"),
            new("Hans Van der linden", "1234", "3245"),
            new("Co Nelen", "1234", "3245")
        };*/

        Patients = ReadUsersFromJson();
    }

    private List<Patient> ReadUsersFromJson()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "AllUsers.json");

        var returnAllUsersFromText = File.ReadAllText(path);

        var data = JsonConvert.DeserializeObject<List<Patient>>(returnAllUsersFromText) ??
                   throw new InvalidOperationException();

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
        foreach (var p in Patients)
        {
            if (p.Password == patient.Password && p.UserId == patient.UserId)
            {
                patient.Username = p.Username;
                return true;
            }
        }

        return false;
    }

    public JObject[] GetPatientDataAsJObjects()
    {
        var jObjects = new JObject[Patients.Count];

        for (var i = 0; i < Patients.Count; i++)
        {
            jObjects[i] = Patients[i].GetPatientAsJObject();
        }

        return jObjects;
    }

    public JObject[] GetPatientSessionsAsJObjects(string userId, string pathString)
    {
        pathString = Path.Combine(pathString.Substring(0, pathString.LastIndexOf("bin")), "allSessions", userId);

        var jObjects = new JObject[Directory.GetFiles(pathString).Length];

        for (var i = 0; i < Directory.GetFiles(pathString).Length; i++)
        {
            using var reader = new JsonTextReader(File.OpenText(Directory.GetFiles(pathString)[i]));
            jObjects[i] = (JObject)JToken.ReadFrom(reader);
        }

        return jObjects;
    }
}