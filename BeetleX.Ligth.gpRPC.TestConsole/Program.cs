// See https://aka.ms/new-console-template for more information
using BeetleX.Light;
using BeetleX.Light.Logs;
using BeetleX.Ligth.gpRPC;
using BeetleX.Ligth.gpRPC.TestConsole;


RpcServer<ApplicationBase> server = new RpcServer<ApplicationBase>();
server.CertificateFile = "generate.pfx";
server.CertificatePassword = "12345678";
server.RegisterMessages<RegisterReq>();
server.Options.AddLogOutputHandler<LogOutputToConsole>();
server.Options.LogLevel = LogLevel.Trace;
server.Start();
await Task.Delay(3000);

RpcClient client = "tcp://localhost:8081";
client.SslServiceName = "beetlex-io.com";
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

    public async Task<UsersResp> Users(UsersReq req)
    {
        UsersResp resp = new UsersResp();
        for (int i = 0; i < req.Count; i++)
        {
            User user = new User();
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