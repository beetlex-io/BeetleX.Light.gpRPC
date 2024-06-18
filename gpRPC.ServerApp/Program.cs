// See https://aka.ms/new-console-template for more information
using BeetleX.Light;
using BeetleX.Light.Logs;
using BeetleX.Light.Protocols;
using BeetleX.Light.gpRPC;
using gpRPC.Messages;
using System;

Constants.MemorySegmentMinSize = 1024 * 16;
Constants.MemorySegmentMaxSize = 1024 * 16;
ThreadPool.SetMinThreads(Environment.ProcessorCount * 2, Environment.ProcessorCount * 2);

Console.WriteLine("BeetleX gpRPC server");
int port = 0;
while (port < 1 || port > 60000)
{
    Console.Write($"Enter listen port(1-60000):");
    var inputPort = Console.ReadLine();
    int.TryParse(inputPort, out port);
}

Console.WriteLine("");
Console.WriteLine($"Info [Port:{port}] enter to start...");
Console.ReadLine();

RpcServer netServer = new RpcServer();
netServer.Options.LogLevel = LogLevel.Info;
netServer.Options.AddLogOutputHandler<LogOutputToConsole>();
netServer.RegisterMessages<UsersReq>();

netServer.Options.SetDefaultListen(p =>
{
    p.Port = port;
    // p.EnabledSSL("generate.pfx", "12345678", System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls11);
});
netServer.Start();
await netServer.Debug();

