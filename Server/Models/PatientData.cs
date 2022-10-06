﻿namespace RemoteHealthcare.Server.Models;

public class PatientData
{
    public List<Patient> Patients { get; set; }

    public PatientData()
    {
        Patients = new List<Patient>();
        Patients.Add(new Patient("test", "test", "test"));
    }

    public bool MatchLoginData(Patient patient)
    {
        foreach (var varPatient in Patients)
        {
            if (varPatient.Username.Equals(patient.Username) && varPatient.Password.Equals(patient.Password) && 
                varPatient.UserId.Equals(patient.UserId))
            {
                return true;
            }
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