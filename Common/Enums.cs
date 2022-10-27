namespace RemoteHealthcare.Common
{
    public class OperationCodes
    {
        public static readonly string Chat = "chat";
        public static readonly string Bikedata = "bikedata";
        public static readonly string Login = "login";
        public static readonly string Status = "status";
        public static readonly string Disconnect = "disconnect";
        public static readonly string Users = "users";
        public static readonly string SessionStart = "session start";
        public static readonly string SessionStop = "session stop";
        public static readonly string EmergencyStop = "emergency stop";
        public static readonly string GetPatientData = "get patient data";
        public static readonly string GetPatientSesssions = "get patient sessions";
        public static readonly string SetResistance = "set resitance";
    }

    public enum StatusCodes
    {
        OK = 200, // Standard OK response.
        CREATED = 201, // Indicates that the requested file has been created.
        ACCEPTED = 202, // Request has been accepted.
        BAD_REQUEST = 400, // Indicates that there was a syntax error in the request.
        UNAUTHORIZED = 401, // Indicates that the user needs to login to perform the requested action.
        FORBIDDEN = 403, // Indicates that the requested action is not allowed for that user.
        NOT_FOUND = 404, // Indicates that the requested data is not found.
        INTERNAL_SERVER_ERROR = 500 // Indicates that the server ran into an error it doesn't know how to handle.
    }
}