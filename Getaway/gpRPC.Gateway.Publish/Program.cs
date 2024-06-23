using BeetleX.Light.Clients;
using BeetleX.Light.gpRPC;
using BeetleX.Light.Logs;
using gpRPC.Gateway.Messages;

RpcClient client = "tcp://localhost:8081";
client.SslServiceName = "beetlex-io.com";
client.AddLogOutputHandler<LogOutputToConsole>();
client.RegisterMessages<RegisterReq>();
client.LogLevel = LogLevel.Debug;
client.UserName = "admin";
client.Password = "123456";
var userhandler = client.Create<IUserHandler>();
var search = new SearchUserReq();
search.Size = 10;
search.MatchName = "*ab";
var result = await userhandler.Search(search);
await NetClientDebug.Default.Debug();