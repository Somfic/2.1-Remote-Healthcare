using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RemoteHealthcare.Common.Socket.Client;


namespace RemoteHealthcare.Common;


public abstract class DAbstract
{
    public string ToJson() {
        return JsonConvert.SerializeObject(this);
    }
}

//it the abstract class for eyery Datapacket
public class DataPacket<T> : DAbstract where T : DAbstract
{
    public string OpperationCode;
    public T data;
}

//DataPacket is the fundament for the specific packets
public class DataPacket : DAbstract
{
    public string OpperationCode;
    
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    
    private JObject data;

    public T GetData<T>() where T : DAbstract
    {
        return this.data.ToObject<T>();
    }
}
    
public class ErrorPacket : DAbstract
{
    public StatusCodes statusCode;
}

public class LoginPacketRequest : DAbstract
{
    public string username;
    public string password;
    public bool isDoctor;
}

public class LoginPacketResponse : DAbstract
{
    public string userId;
    public StatusCodes statusCode;
    public string message;
}

public class ConnectedClientsPacketRequest : DAbstract
{
    public string requester;
}

public class ConnectedClientsPacketResponse : DAbstract
{
    public StatusCodes statusCode;
    public string connectedIds;
}

public class ChatPacketRequest : DAbstract
{
    public string senderId;
    public string receiverId;
    public string message;
}

public class ChatPacketResponse : DAbstract
{
    public string senderId;
    public StatusCodes statusCode;
    public string message;
}

public class SessionStartPacketRequest : DAbstract
{
    public string selectedPatient;
}

public class SessionStartPacketResponse : DAbstract
{
    public StatusCodes statusCode;
    public string message;
}

public class SessionStopPacketRequest : DAbstract
{
    public string selectedPatient;
}

public class SessionStopPacketResponse : DAbstract
{
    public StatusCodes statusCode;
    public string message;
}

public class EmergencyStopPacketRequest : DAbstract
{
    //TODO
}

public class EmergencyStopPacketResponse : DAbstract
{
    public StatusCodes statusCode;
    public string message;
}
public class DisconnectPacketRequest : DAbstract
{
    //TODO: voorlopig leeg laten
}

public class SetResistancePacket : DAbstract
{
    public string receiverId;
    public int resistance;
}
public class SetResistanceResponse : DAbstract
{
    public StatusCodes statusCode;
    public int resistance;
}

public class DisconnectPacketResponse : DAbstract
{
    public StatusCodes statusCode;
    public string message;
}

public class BikeDataPacket : DAbstract
{
    public string SessionId;
    public float distance;
    public float speed;
    public int heartRate;
    public TimeSpan elapsed;
    public string deviceType;
    public string id;
}

public class BikeDataPacketDoctor : DAbstract
{
    public float distance;
    public float speed;
    public int heartRate;
    public TimeSpan elapsed;
    public string id;
}

public class GetAllPatientsDataRequest : DAbstract
{
    //TODO voorlopig leeg laten
}

public class GetAllPatientsDataResponse : DAbstract
{
    public StatusCodes statusCode;
    public string message;
    public JObject[] JObjects;
}

public class AllSessionsFromPatientRequest : DAbstract
{
    public StatusCodes statusCode;
    public string userId;
}

public class AllSessionsFromPatientResponce : DAbstract
{
    public StatusCodes statusCode;
    public JObject[] JObjects;
    public string message;
}