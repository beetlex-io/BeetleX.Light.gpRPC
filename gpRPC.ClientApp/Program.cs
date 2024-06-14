// See https://aka.ms/new-console-template for more information
using BeetleX.Light;
using BeetleX.Light.Clients;
using BeetleX.Light.Extension;
using BeetleX.Light.Logs;
using BeetleX.Light.Protocols;
using BeetleX.Ligth.gpRPC;
using gpRPC.Messages;
using System.Xml;

Constants.MemorySegmentMinSize = 1024 * 16;
Constants.MemorySegmentMaxSize = 1024 * 16;
ThreadPool.SetMinThreads(Environment.ProcessorCount * 2, Environment.ProcessorCount * 2);

Console.WriteLine("BeetleX gpRPC client");
Console.Write("Enter server address(tcp://host:port):");
var uriStr = Console.ReadLine();
var uri = new Uri(uriStr);
int connections = 0;
while (connections < 1 || connections > 100)
{
    Console.Write("Enter connections(1-100):");
    var conns = Console.ReadLine();
    int.TryParse(conns, out connections);
}
int users = 0;
while (users < 1 || users > 500)
{
    Console.Write("Enter users for connection(1-500):");
    var usersStr = Console.ReadLine();
    int.TryParse(usersStr, out users);
}

Console.WriteLine("");
Console.WriteLine($"Info [host:{uri}] [connections: {connections}] [users for connection: {users}] enter to start...");
Console.ReadLine();

List<RpcClient> clients = new List<RpcClient>();
for (int i = 0; i < connections; i++)
{
    RpcClient client = uriStr;
    //client.SslServiceName = "beetlex-io.com";
    client.AddLogOutputHandler<LogOutputToConsole>();
    client.RegisterMessages<UsersReq>();
    client.LogLevel = LogLevel.Info;
    client.TimeOut = 10000;
    clients.Add(client);
}

foreach (var client in clients)
{
    IUserHandler handler = client.Create<IUserHandler>();
    for (int i = 0; i < users; i++)
    {
        var task = Test.Register(handler, client);
        task = Test.Users(handler, client);
    }
}

await NetClientDebug.Default.Debug();


public class Test
{
    public static async Task Users(IUserHandler handler, RpcClient client)
    {
        Random ran = new Random();
        while (true)
        {
            try
            {
                var userReq = new UsersReq();
                userReq.Count = (uint)ran.Next(1, 30);
                var users = await handler.Users(userReq);
            }
            catch (Exception e)
            {
                client.GetLoger(LogLevel.Error)?.WriteException(client, "TEST", "Run", e);
            }
        }
    }
    public static async Task Register(IUserHandler handler, RpcClient client)
    {
        Random ran = new Random();
        while (true)
        {
            try
            {
                var register = new RegisterReq();
                register.Email = "henryfan@Msn.com";
                register.Address = "longdong";
                register.City = "guangzhou";
                register.FirstName = "fan";
                register.LastName = "henryfan";
                register.Password = "123456";
                var resp = await handler.Register(register);
            }
            catch (Exception e)
            {
                client.GetLoger(LogLevel.Error)?.WriteException(client, "TEST", "Run", e);
            }
        }
    }
}