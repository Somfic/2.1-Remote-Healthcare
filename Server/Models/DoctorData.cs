using RemoteHealthcare.Common.Logger;

namespace RemoteHealthcare.Server.Models;
public class DoctorData
{
    private readonly Log _log = new(typeof(DoctorData));
    
    public Doctor _doctor { get; set; }

    public bool MatchLoginData(Doctor d)
    {
        if (_doctor == null)
            return false;
        
        if (_doctor.Username.Equals(d.Username) && _doctor.Password.Equals(d.Password) &&
            _doctor.UserId.Equals(d.UserId))
            return true;
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