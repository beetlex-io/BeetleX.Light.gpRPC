// See https://aka.ms/new-console-template for more information
using BeetleX.Light.Clients;
using BeetleX.Light.gpRPC;
using BeetleX.Light.Logs;
using gpRPC.Gateway.Messages;

RpcClient client = "tcp://localhost:8081";
client.SslServiceName = "beetlex-io.com";
client.AddLogOutputHandler<LogOutputToConsole>();
client.RegisterMessages<RegisterReq>()
      .RegisterMessages<UserHandler>();
client.LogLevel = LogLevel.Trace;
client.UserName = "admin";
client.Password = "123456";
await client.Subscribe<RegisterReq, SearchUserReq>();
await NetClientDebug.Default.Debug();

[RpcService]
public class UserHandler : IUserHandler
{
    public async Task<SearchUserResp> Search(SearchUserReq req)
    {
        SearchUserResp resp = new SearchUserResp();
        for (int i = 0; i < req.Size; i++)
        {
            var user = new gpRPC.Gateway.Messages.User();
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
    public async Task<RegisterResp> Register(RegisterReq req)
    {
        RegisterResp resp = new RegisterResp();
        resp.Success = true;
        resp.Time = DateTime.Now.Ticks;
        return resp;
    }

    public Task SetTime(SetTimeReq req)
    {
        throw new NotImplementedException();
    }


}