namespace RemoteHealthcare.Server.Models;

namespace RemoteHealthcare.Server.Models;

public class DoctorData
{
    private readonly Log _log = new(typeof(DoctorData));
    
    private Doctor _doctor { get; set; }

    public DoctorData()
    {
        this._doctor = new Doctor("Piet", "dhrPiet", "Dhr145");
    }

    public bool MatchLoginData(Doctor d)
    {
        if (_doctor.username.Equals(d.username) && _doctor.password.Equals(d.password) &&
            _doctor.userId.Equals(d.userId))
            return true;
        else 
            return false;
    }

    public void SaveDoctorData()
    {
        string folderName = Environment.CurrentDirectory;
        _log.Debug(folderName);
        folderName = Path.Combine(folderName.Substring(0, folderName.LastIndexOf("bin")) + "DoctorDataFiles");
        _doctor.SaveSessionData(folderName);
    }
}