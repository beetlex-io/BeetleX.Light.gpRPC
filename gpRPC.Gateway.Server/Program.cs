// See https://aka.ms/new-console-template for more information
using BeetleX.Light.gpRPC;
using BeetleX.Light.gpRPC.Gateway;
using BeetleX.Light.Logs;
using gpRPC.Gateway.Messages;

RpcGatewayServer server = new RpcGatewayServer();
server.CertificateFile = "generate.pfx";
server.CertificatePassword = "12345678";
server.RegisterMessages<RegisterReq>();
server.Options.AddLogOutputHandler<LogOutputToConsole>();
server.Options.LogLevel = LogLevel.Debug;

server.Start();
await server.Debug();
