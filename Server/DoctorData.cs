namespace RemoteHealthcare.CentralServer;

public class DoctorData
{
    public Doctor doctor { get; set; }

    public DoctorData()
    {
        this.doctor = new Doctor("Piet", "dhrPiet", "Dhr145");
    }

    public bool MatchLoginData(Doctor d)
    {
        if (doctor.username.Equals(d.username) && doctor.password.Equals(d.password) &&
            doctor.userId.Equals(d.userId))
            return true;
        else 
            return false;
    }

    public void SaveDoctorData()
    {
        string folderName = Environment.CurrentDirectory;
        Console.WriteLine(folderName);
        folderName = Path.Combine(folderName.Substring(0, folderName.LastIndexOf("bin")) + "DoctorDataFiles");
        doctor.SaveSessionData(folderName);
    }
}