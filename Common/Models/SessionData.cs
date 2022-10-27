using MvvmHelpers;
using Newtonsoft.Json.Linq;

namespace RemoteHealthcare.Common.Models;

[Serializable]
public class SessionData : ObservableObject
{
    public string SessionId { get; set; }
    public string DeviceType { get; set; }
    public string Id { get; set; }
    public List<SessionMiniData> MiniDatas {get; set;}

    public SessionData(string sessionID, string deviceType, string id)
    {
        MiniDatas = new List<SessionMiniData>();

        SessionId = sessionID;
        DeviceType = deviceType;
        Id = id;
        
    }

    /// <summary>
    /// It adds a new mini data to the session if the session id, device type and id are the same as the ones in the data
    /// </summary>
    /// <param name="JObject">This is the data that is sent from the device.</param>
    /// <returns>
    /// A boolean value.
    /// </returns>
    public bool addMiniData(JObject data)
    {
        if (!SessionId.Equals(data["sessionId"].ToObject<string>()) || !DeviceType.Equals(data["deviceType"].ToObject<string>()) || !Id.Equals(data["id"].ToObject<string>()))
        {
            return false;
        }
        MiniDatas.Add(new SessionMiniData(data["speed"].ToObject<int>(), data["distance"].ToObject<int>(), data["heartRate"].ToObject<int>(), data["elapsed"].ToObject<int>()));
        return true;
    }

    /// <summary>
    /// It adds a new mini data to the session if the session ID, device type and ID are correct
    /// </summary>
    /// <param name="sessionID">The session ID of the session you want to add data to.</param>
    /// <param name="speed">speed in km/h</param>
    /// <param name="distance">distance in meters</param>
    /// <param name="heartrate">the heartrate of the user</param>
    /// <param name="elapsed">time in seconds since the start of the session</param>
    /// <param name="deviceType">The type of device that the data is coming from.</param>
    /// <param name="id">The id of the device that is sending the data.</param>
    /// <returns>
    /// A boolean value.
    /// </returns>
    public bool addMiniData(string sessionID, int speed, int distance, int heartrate, int elapsed, string deviceType, string id)
    {
        if (!SessionId.Equals(sessionID) || !DeviceType.Equals(deviceType) || !Id.Equals(id))
        {
            return false;
        }
        MiniDatas.Add(new SessionMiniData(speed, distance, heartrate, elapsed));
        return true;
    }

    public override string ToString()
    {
        return $"{SessionId} - {Id}";
    }
}