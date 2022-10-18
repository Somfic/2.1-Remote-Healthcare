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
        return this._data.ToObject<T>();
    }
}
    
public class ErrorPacket : DAbstract
{
    public StatusCodes StatusCode;
}

public class LoginPacketRequest : DAbstract
{
    public string Username;
    public string Password;
    public bool IsDoctor;
}

public class LoginPacketResponse : DAbstract
{
    public string UserId;
    public StatusCodes StatusCode;
    public string Message;
}

public class ConnectedClientsPacketRequest : DAbstract
{
    public string OperationCode;
}

public class ConnectedClientsPacketResponse : DAbstract
{
    public StatusCodes StatusCode;
    public string ConnectedIds;
}

public class ChatPacketRequest : DAbstract
{
    public string SenderId;
    public string ReceiverId;
    public string Message;
}

public class ChatPacketResponse : DAbstract
{
    public string SenderId;
    public StatusCodes StatusCode;
    public string Message;
}

public class SessionStartPacketRequest : DAbstract
{
    //TODO: voorlopig leeg laten
}

public class SessionStartPacketResponse : DAbstract
{
    public StatusCodes StatusCode;
    public string Message;
}

public class SessionStopPacketRequest : DAbstract
{
    //TODO: voorlopig leeg laten
}

public class SessionStopPacketResponse : DAbstract
{
    public StatusCodes StatusCode;
    public string Message;
}

public class EmergencyStopPacketRequest : DAbstract
{
    //TODO: voorlopig leeg laten
}

public class EmergencyStopPacketResponse : DAbstract
{
    public StatusCodes StatusCode;
    public string Message;
}
public class DisconnectPacketRequest : DAbstract
{
    //TODO: voorlopig leeg laten
}

public class DisconnectPacketResponse : DAbstract
{
    public StatusCodes StatusCode;
    public string Message;
}