namespace RemoteHealthcare.Common
{
    public static class OperationCodes
    {
        public readonly static String CHAT = "chat";
        public readonly static String BIKEDATA = "bikedata";
        public readonly static String LOGIN = "login";
        public readonly static String STATUS = "status";
        public readonly static String DISCONNECT = "disconnect";
        public readonly static String USERS = "users";
        public readonly static String SESSION_START = "session start";
        public readonly static String SESSION_STOP = "session stop";
        public readonly static String EMERGENCY_STOP = "emergency stop";
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