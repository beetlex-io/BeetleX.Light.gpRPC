using BeetleX.Light.Clients;
using BeetleX.Light.gpRPC;
using BeetleX.Light.Logs;
using gpRPC.Gateway.Messages;

RpcClient client = "tcp://localhost:8081";
client.SslServiceName = "beetlex-io.com";
client.AddLogOutputHandler<LogOutputToConsole>();
client.RegisterMessages<RegisterReq>();
client.LogLevel = LogLevel.Trace;
client.UserName = "admin";
client.Password = "123456";
var userhandler = client.Create<IUserHandler>();
var search = new SearchUserReq();
search.Size = 10;
search.MatchName = "*ab";
var result = await userhandler.Search(search);
var retReq = new RegisterReq();
retReq.Address = $"guangzhouLongdong";
retReq.City = $"guzngzhou";
retReq.Email = "henryfan@msn.com";
retReq.FirstName = $"fan";
retReq.LastName = $"henry";
retReq.Password = "122";
var regResp = await userhandler.Register(retReq);

await NetClientDebug.Default.Debug();