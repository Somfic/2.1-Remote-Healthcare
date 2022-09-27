namespace Common;


    public class JsonFile
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
    }
