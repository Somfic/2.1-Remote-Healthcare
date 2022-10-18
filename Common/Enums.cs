namespace RemoteHealthcare.Common
{
    public static class OperationCodes
    {
        public readonly static String Chat = "chat";
        public readonly static String Bikedata = "bikedata";
        public readonly static String Login = "login";
        public readonly static String Status = "status";
        public readonly static String Disconnect = "disconnect";
        public readonly static String Users = "users";
        public readonly static String SessionStart = "session start";
        public readonly static String SessionStop = "session stop";
        public readonly static String EmergencyStop = "emergency stop";
    }

    public enum StatusCodes
    {
        Ok = 200, // Standard OK response.
        Created = 201, // Indicates that the requested file has been created.
        Accepted = 202, // Request has been accepted.
        BadRequest = 400, // Indicates that there was a syntax error in the request.
        Unauthorized = 401, // Indicates that the user needs to login to perform the requested action.
        Forbidden = 403, // Indicates that the requested action is not allowed for that user.
        NotFound = 404, // Indicates that the requested data is not found.
        InternalServerError = 500 // Indicates that the server ran into an error it doesn't know how to handle.
    }
}