using RemoteHealthcare.Common.Logger;
using RemoteHealthcare.Common.Models;

namespace RemoteHealthcare.Server.Models;

public class DoctorData
{
    private readonly Log _log = new(typeof(DoctorData));

    public Doctor Doctor { get; set; }

    public DoctorData()
    {
    }

    /// <summary>
    /// It checks if the given doctor is null or if the username, password, and userid are not equal to the doctor's username,
    /// password, and userid.
    /// </summary>
    /// <param name="d">The doctor object that is being passed in.</param>
    /// <returns>
    /// A boolean value.
    /// </returns>
    public bool MatchLoginData(Doctor d)
    {
        if ((Doctor == null) || (!Doctor.Username.Equals(d.Username) && !Doctor.Password.Equals(d.Password) &&
                                 !Doctor.UserId.Equals(d.UserId)))
        {
            return false;
        }

        return true;
    }
}