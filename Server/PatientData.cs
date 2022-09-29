namespace RemoteHealthcare.CentralServer;

public class PatientData
{
    public List<Patient> _patients { get; set; }

    public PatientData()
    {
        _patients = new List<Patient>();
    }

    public bool MatchLoginData(Patient patient)
    {
        foreach (var varPatient in _patients)
        {
            if (varPatient.username.Equals(patient.username) && varPatient.password.Equals(patient.password) && varPatient.userId.Equals(patient.userId))
            {
                return true;
            }
        }
        return false;
    }

    public void SavePatientData()
    {
        string folderName = Environment.CurrentDirectory;
        Console.WriteLine(folderName);
        folderName = Path.Combine(folderName.Substring(0, folderName.LastIndexOf("bin")) + "PatientDataFiles");
        foreach (var patient in _patients)
        {
            patient.SaveSessionData(folderName);
        }
    }
}