namespace RemoteHealthcare.CentralServer;

public class PatientData
{
    private Dictionary<string, string> _loginData { get; set; }
    private List<Patient> _patients { get; set; }

    public PatientData()
    {
        _loginData = new Dictionary<string, string>();
        _patients = new List<Patient>();
    }

    public bool MatchLoginData(Patient patient)
    {
        if (_patients.Contains(patient))
        {
            return true;
        }

        return false;
    }
}