using BeetleX.Light;
using BeetleX.Light.Clients;
using BeetleX.Light.Logs;
using BeetleX.Light.Protocols;
using BeetleX.Light.gpRPC.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Light.gpRPC
{
    public class RpcSession : ISession
    {
        public RpcSession()
        {

        }
        public string Name { get; set; }

        public AuthenticationType Authentication { get; set; } = AuthenticationType.None;

        public NetContext NetContext { get; private set; }

        public RpcServer Server { get; internal set; }

        public User User { get; internal set; }

        public virtual void Connected(NetContext context)
        {
            NetContext = context;
            Server = (RpcServer)context.Server;
        }

        public virtual void Dispose(NetContext context)
        {
            NetContext = null;
        }

        protected void Send(object message)
        {
            NetContext.Send(message);
        }

        public Task Receive(NetContext context, object message)
        {

            RpcMessage rpcmsg = (RpcMessage)message;

            if (rpcmsg.Type == 2000000003u)
            {
                LoginReq req = (LoginReq)rpcmsg.Body;
                RpcMessage resp = new RpcMessage();
                resp.Identifier = rpcmsg.Identifier;
                var user = Server.UserManager.GetUser(req.UserName);
                if (user != null && req.Password == user.Password)
                {
                    User = user;
                    Authentication = AuthenticationType.Security;
                    Success success = new Success();
                    resp.Body = success;
                    context.GetLoger(LogLevel.Debug)?.Write(context, "gpRPCSession", "Login", "Success");
                }
                else
                {
                    Error error = new Error();
                    error.ErrorCode = RpcException.INVALID_NAME_OR_PASSWORD;
                    error.ErrorMessage = "Invalid user name or password!";
                    resp.Body = error;
                    context.GetLoger(LogLevel.Warring)?.Write(context, "gpRPCSession", "Login", error.ErrorMessage);
                }
                NetContext?.Send(resp);
                return Task.CompletedTask;
            }
            else if (rpcmsg.Type == 2000000004u)
            {
                RpcMessage resp = new RpcMessage();
                resp.Identifier = rpcmsg.Identifier;
                Error error = new Error();
                error.ErrorCode = RpcException.SUBSCRIBER_NOT_SUPPORT;
                error.ErrorMessage = "Not support!";
                resp.Body = error;
                NetContext?.Send(resp);
                return Task.CompletedTask;
            }
            else
            {
                if (User == null || !User.Check(rpcmsg.Type))
                {
                    RpcMessage resp = new RpcMessage();
                    resp.Identifier = rpcmsg.Identifier;
                    Error error = new Error();
                    error.ErrorCode = RpcException.INVALID_CONNECTION;
                    error.ErrorMessage = "Invalid connection!";
                    resp.Body = error;
                    NetContext?.Send(resp);
                    NetContext?.GetLoger(LogLevel.Warring)?.Write(context, "gpRPCSession", "Invoke", "Invalid connection");
                    NetContext?.Dispose();
                    return Task.CompletedTask;
                }
                else
                {
                    return OnReceiveMessage(rpcmsg);
                }
            }
        }

        protected virtual async Task OnReceiveMessage(RpcMessage req)
        {

            RpcMessage resp = new RpcMessage();
            resp.Identifier = req.Identifier;
            var method = ServiceMethodHandlers.Default.GetMethod(req.Body.GetType());
            if (method != null)
            {
                try
                {
                    Task task = (Task)method.Method.Invoke(method.Service, new object[] { req.Body });
                    await task;
                    var result = method.ResultProperty.GetValue(task);
                    resp.Body = result;
                    NetContext.GetLoger(LogLevel.Debug)?.Write(NetContext, "gpRPCSession", "InvokeSuccess", $"{req.Body.GetType().Name}");

                }
                catch (Exception e_)
                {
                    Error error = new Error();
                    error.ErrorCode = RpcException.METHOD_INVOKE_ERROR;
                    error.ErrorMessage = e_.Message;
                    error.StackTrace = e_.StackTrace;
                    resp.Body = error;
                    NetContext?.GetLoger(LogLevel.Error)?.Write(NetContext, "gpRPCSession", "InvokeError", $"{req.Body.GetType().Name} invok error {e_.Message} {e_.StackTrace}!");
                }

            }
            else
            {
                Error error = new Error();
                error.ErrorCode = RpcException.METHOD_NOTFOUND;
                error.ErrorMessage = $"{req.Body.GetType().Name} handler not found!";
                resp.Body = error;
                NetContext?.GetLoger(LogLevel.Warring)?.Write(NetContext, "gpRPCSession", "InvokeError", $"{req.Body.GetType().Name} handler not found!");
            }
            NetContext?.Send(resp);
        }

        public virtual void ReceiveCompleted(int bytes)
        {

        }

        public virtual void SendCompleted(int bytes)
        {

        }




    }
}
