﻿using Newtonsoft.Json;
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


public class LoginPacket : DAbstract
{
    public string username;
    public string password;
}

public class LoginResponsePacket : DAbstract
{
    public StatusCodes statusCode;
    public string message;
}

public class ChatPacket : DAbstract
{
    public string message;
}

public class ChatResponse : DAbstract
{
    public StatusCodes statusCode;
    public string message;
}

public class SessionStartPacket : DAbstract
{
    //voorlopig leeg laten
}

public class SessionStartResponse : DAbstract
{
    public StatusCodes statusCode;
    public string message;
}

public class SessionStopPacket : DAbstract
{
    //voorlopig leeg laten
}

public class SessionStopResponse : DAbstract
{
    public StatusCodes statusCode;

    public string message;
}

/*public class JsonFile
{
    public int StatusCode { get; set; }
    public string OppCode { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    public JsonData Data { get; set; }
}

public class JsonData
{
    public uint AutenticationID { get; set; }
    public string Password { get; set; }
    public string Title { get; set; }
    public string FileName { get; set; }
    public string ChatMessage { get; set; }
    public string Content { get; set; }
    public bool BikeDataHistory { get; set; }
}*/