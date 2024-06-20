// See https://aka.ms/new-console-template for more information
using BeetleX.Light;
using BeetleX.Light.Logs;
using BeetleX.Light.gpRPC;
using BeetleX.Light.gpRPC.TestConsole;


RpcServer server = new RpcServer();
server.CertificateFile = "generate.pfx";
server.CertificatePassword = "12345678";
server.RegisterMessages<RegisterReq>();
server.Options.AddLogOutputHandler<LogOutputToConsole>();
server.Options.LogLevel = LogLevel.Trace;
//server.UserManager.GetUser("admin")
//    .SetRight<RegisterReq>();
server.Start();
await Task.Delay(3000);

RpcClient client = "tcp://localhost:8081";
client.SslServiceName = "beetlex-io.com";
client.AddLogOutputHandler<LogOutputToConsole>();
client.RegisterMessages<RegisterReq>();
client.LogLevel = LogLevel.Trace;
client.UserName = "admin";
client.Password = "123456";
IUserHandler handler = client.Create<IUserHandler>();
RegisterReq req = new RegisterReq();
req.Address = $"guangzhouLongdong";
req.City = $"guzngzhou";
req.Email = "henryfan@msn.com";
req.FirstName = $"fan";
req.LastName = $"henry";
req.Password = "122";
var resp = await handler.Register(req);
client.GetLoger(LogLevel.Info)?.Write(client, "Service", "Invoke", "Register completed");
UsersReq usersreq = new UsersReq();
usersreq.Count = 5;
await handler.Users(usersreq);
client.GetLoger(LogLevel.Info)?.Write(client, "Service", "Invoke", "Users completed");
SetTimeReq setTimeReq = new SetTimeReq();
setTimeReq.Time = DateTime.Now.Ticks;
await handler.SetTime(setTimeReq);
client.GetLoger(LogLevel.Info)?.Write(client, "Service", "Invoke", "SetTime completed");
await server.Debug();

[RpcService]
public class UserHandler : IUserHandler
{
    public async Task<RegisterResp> Register(RegisterReq req)
    {
        RegisterResp resp = new RegisterResp();
        resp.Success = true;
        resp.Time = DateTime.Now.Ticks;
        return resp;
    }

    public Task SetTime(SetTimeReq req)
    {
        return Task.CompletedTask;
    }

    public async Task<UsersResp> Users(UsersReq req)
    {
        UsersResp resp = new UsersResp();
        for (int i = 0; i < req.Count; i++)
        {
            BeetleX.Light.gpRPC.TestConsole.User user = new BeetleX.Light.gpRPC.TestConsole.User();
            user.Address = $"guangzhouLongdong{i}";
            user.City = $"guzngzhou{i}";
            user.Email = "henryfan@msn.com";
            user.FirstName = $"fan{i}";
            user.LastName = $"henry{i}";
            user.Password = "122".PadLeft(i, 'a');
            user.Remark = $"{i}";
            resp.Items.Add(user);
        }
        return resp;
    }
}