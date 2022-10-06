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
    public StatusCodes statusCode;
    public string message;
}

public class ChatPacketRequest : DAbstract
{
    public string message;
}

public class ChatPacketResponse : DAbstract
{
    public StatusCodes statusCode;
    public string message;
}

public class SessionStartPacketRequest : DAbstract
{
    //voorlopig leeg laten
}

public class SessionStartPacketResponse : DAbstract
{
    public StatusCodes statusCode;
    public string message;
}

public class SessionStopPacketRequest : DAbstract
{
    //voorlopig leeg laten
}

public class SetResistancePacketResponse : DAbstract
{
    public StatusCodes statusCode;
    public string message;
}

public class SessionStopPacketResponse : DAbstract
{
    public StatusCodes statusCode;
    public string message;
}
public class DisconnectPacketRequest : DAbstract
{
    //voorlopig leeg laten
}

public class DisconnectPacketResponse : DAbstract
{
    public StatusCodes statusCode;
    public string message;
}