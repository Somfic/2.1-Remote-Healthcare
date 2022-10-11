using RemoteHealthcare.Server;

var server = new RemoteHealthcare.Server.Server();
await server.StartAsync();
await Task.Delay(-1);