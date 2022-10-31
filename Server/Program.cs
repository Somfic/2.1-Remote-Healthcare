using RemoteHealthcare.Server;

var server = new Server();
await server.StartAsync();
await Task.Delay(-1);