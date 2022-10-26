using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
    public T Data;
}

//DataPacket is the fundament for the specific packets
public class DataPacket : DAbstract
{
    public string OpperationCode;
    
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    
    private JObject _data;

    public T GetData<T>() where T : DAbstract
    {
        return _data.ToObject<T>();
    }
}
    
public class ErrorPacket : DAbstract
{
    public StatusCodes StatusCode;
}

public class LoginPacketRequest : DAbstract
{
    public string UserName;
    public string Password;
    public bool IsDoctor;
}

public class LoginPacketResponse : DAbstract
{
    public string UserId;
    public string UserName;
    public StatusCodes StatusCode;
    public string Message;
}

public class ConnectedClientsPacketRequest : DAbstract
{
    public string Requester;
}

public class ConnectedClientsPacketResponse : DAbstract
{
    public StatusCodes StatusCode;
    public string ConnectedIds;
}

public class ChatPacketRequest : DAbstract
{
    public string SenderId;
    public string SenderName;
    public string ReceiverId;
    public string Message;
}

public class ChatPacketResponse : DAbstract
{
    public string SenderId;
    public string SenderName;
    public StatusCodes StatusCode;
    public string Message;
}

public class SessionStartPacketRequest : DAbstract
{
    public string SelectedPatient;
}

public class SessionStartPacketResponse : DAbstract
{
    public StatusCodes StatusCode;
    public string Message;
}

public class SessionStopPacketRequest : DAbstract
{
    public string SelectedPatient;
}

public class SessionStopPacketResponse : DAbstract
{
    public StatusCodes StatusCode;
    public string Message;
}

public class EmergencyStopPacket : DAbstract
{
    public StatusCodes StatusCode;
    public string ClientId;
    public string Message = "Er is op de noodstop gedrukt, de dokter komt zo spoedig mogelijk bij u";
}
public class DisconnectPacketRequest : DAbstract
{
    //TODO: voorlopig leeg laten
}

public class SetResistancePacket : DAbstract
{
    public string ReceiverId;
    public int Resistance;
}
public class SetResistanceResponse : DAbstract
{
    public StatusCodes StatusCode;
    public int Resistance;
}

public class DisconnectPacketResponse : DAbstract
{
    public StatusCodes StatusCode;
    public string Message;
}

public class BikeDataPacket : DAbstract
{
    public string SessionId;
    public float Distance;
    public float Speed;
    public int HeartRate;
    public TimeSpan Elapsed;
    public string DeviceType;
    public string Id;
}

public class BikeDataPacketDoctor : DAbstract
{
    public float Distance;
    public float Speed;
    public int HeartRate;
    public TimeSpan Elapsed;
    public string Id;
}

public class GetAllPatientsDataRequest : DAbstract
{
    //TODO voorlopig leeg laten
}

public class GetAllPatientsDataResponse : DAbstract
{
    public StatusCodes StatusCode;
    public string Message;
    public JObject[] JObjects;
}

public class AllSessionsFromPatientRequest : DAbstract
{
    public StatusCodes StatusCode;
    public string UserId;
}

public class AllSessionsFromPatientResponce : DAbstract
{
    public StatusCodes StatusCode;
    public JObject[] JObjects;
    public string Message;
}