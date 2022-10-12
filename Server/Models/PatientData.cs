﻿using Newtonsoft.Json.Linq;
using RemoteHealthcare.Common.Logger;

namespace RemoteHealthcare.Server.Models;

public class PatientData
{
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

    /// <summary>
    /// It takes the current directory, removes the last instance of "bin" from it, and then adds "PatientDataFiles" to the
    /// end of it
    /// </summary>
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