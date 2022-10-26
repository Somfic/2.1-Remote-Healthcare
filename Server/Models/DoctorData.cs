using RemoteHealthcare.Common.Logger;

namespace RemoteHealthcare.Server.Models;

public class DoctorData
{
    private readonly Log _log = new(typeof(DoctorData));

    public Doctor Doctor { get; set; }

    public bool MatchLoginData(Doctor d)
    {
        if (Doctor == null)
            return false;

        if (Doctor.Username.Equals(d.Username) && Doctor.Password.Equals(d.Password) &&
            Doctor.UserId.Equals(d.UserId))
            return true;
        return false;
    }

    public void SaveDoctorData()
    {
        var folderName = Environment.CurrentDirectory;
        _log.Debug(folderName);
        folderName = Path.Combine(folderName.Substring(0, folderName.LastIndexOf("bin")) + "DoctorDataFiles");
        Doctor.SaveSessionData(folderName);
    }
}