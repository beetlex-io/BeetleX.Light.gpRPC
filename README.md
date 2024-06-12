# BeetleX.Light.gpRPC
high performance dotnet core google protobuf rpc,supports millions of rps communication
## server
``` csharp
RpcServer<ApplicationBase> server = new RpcServer<ApplicationBase>();
server.Options.SetDefaultListen(o =>
{
    o.Port = 8080;
});
server.RegisterMessages<RegisterReq>();
server.Options.AddLogOutputHandler<LogOutputToConsole>();
server.Options.LogLevel = LogLevel.Trace;
server.Start();
await Task.Delay(3000);
```

## client
``` csharp
RpcClient client = "tcp://localhost:8080";
client.AddLogOutputHandler<LogOutputToConsole>();
client.RegisterMessages<RegisterReq>();
client.LogLevel = LogLevel.Trace;
IUserHandler handler = client.Create<IUserHandler>();
RegisterReq req = new RegisterReq();
req.Address = $"guangzhouLongdong";
req.City = $"guzngzhou";
req.Email = "henryfan@msn.com";
req.FirstName = $"fan";
req.LastName = $"henry";
req.Password = "122";
var resp = await handler.Register(req);
```
## performance
### 1 connection 100 concurrency
![image](https://github.com/beetlex-io/BeetleX.Light.gpRPC/assets/2564178/4fd533f0-4e1d-47cf-a2d9-48a51fe5085c)

### 20 connections 2000 concurrency
![image](https://github.com/beetlex-io/BeetleX.Light.gpRPC/assets/2564178/082828f1-e461-4672-85ab-fc93ec38fa2e)
