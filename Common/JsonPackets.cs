using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RemoteHealthcare.Common;

public abstract class DAbstract
{
    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }
}

//it the abstract class for eyery Datapacket
public class DataPacket<T> : DAbstract where T : DAbstract
{
    public T Data;
    public string OpperationCode;
}

//DataPacket is the fundament for the specific packets
public class DataPacket : DAbstract
{
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    private JObject _data;

    public string OpperationCode;

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
    public bool IsDoctor;
    public string Password;
    public string UserName;
}

public class LoginPacketResponse : DAbstract
{
    public string Message;
    public StatusCodes StatusCode;
    public string UserId;
    public string UserName;
}

public class ConnectedClientsPacketRequest : DAbstract
{
    public string Requester;
}

public class ConnectedClientsPacketResponse : DAbstract
{
    public string ConnectedIds;
    public StatusCodes StatusCode;
}

public class ChatPacketRequest : DAbstract
{
    public string Message;
    public string ReceiverId;
    public string SenderId;
    public string SenderName;
}

public class ChatPacketResponse : DAbstract
{
    public string Message;
    public string SenderId;
    public string SenderName;
    public StatusCodes StatusCode;
}

public class SessionStartPacketRequest : DAbstract
{
    public string SelectedPatient;
}

public class SessionStartPacketResponse : DAbstract
{
    public string Message;
    public StatusCodes StatusCode;
}

public class SessionStopPacketRequest : DAbstract
{
    public string SelectedPatient;
}

public class SessionStopPacketResponse : DAbstract
{
    public string Message;
    public StatusCodes StatusCode;
}

public class EmergencyStopPacket : DAbstract
{
    public string ClientId;
    public string Message = "Er is op de noodstop gedrukt, de dokter komt zo spoedig mogelijk bij u";
    public StatusCodes StatusCode;
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
    public int Resistance;
    public StatusCodes StatusCode;
}

public class DisconnectPacketResponse : DAbstract
{
    public string Message;
    public StatusCodes StatusCode;
}

public class BikeDataPacket : DAbstract
{
    public string DeviceType;
    public float Distance;
    public TimeSpan Elapsed;
    public int HeartRate;
    public string Id;
    public string SessionId;
    public float Speed;
}

public class BikeDataPacketDoctor : DAbstract
{
    public float Distance;
    public TimeSpan Elapsed;
    public int HeartRate;
    public string Id;
    public float Speed;
}

public class GetAllPatientsDataRequest : DAbstract
{
    //TODO voorlopig leeg laten
}

public class GetAllPatientsDataResponse : DAbstract
{
    public JObject[] JObjects;
    public string Message;
    public StatusCodes StatusCode;
}

public class AllSessionsFromPatientRequest : DAbstract
{
    public StatusCodes StatusCode;
    public string UserId;
}

public class AllSessionsFromPatientResponce : DAbstract
{
    public JObject[] JObjects;
    public string Message;
    public StatusCodes StatusCode;
}