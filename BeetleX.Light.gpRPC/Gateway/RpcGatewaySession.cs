using BeetleX.Light;
using BeetleX.Light.gpRPC.Messages;
using BeetleX.Light.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BeetleX.Light.gpRPC.Gateway
{
    public class RpcGatewaySession : ISession
    {
        public string Name { get; set; }
        public AuthenticationType Authentication { get; set; }

        public RpcGatewayServer Server { get; set; }

        public NetContext NetContext { get; set; }

        public GatewayApplicatoin Application { get; set; }

        public User User { get; private set; }

        public void Connected(NetContext context)
        {
            this.Server = (RpcGatewayServer)context.Server;
            NetContext = context;
            this.Application = (GatewayApplicatoin)context.Server.Application;
        }

        public void Dispose(NetContext context)
        {
            Server.MessageLoadBalancerTable.Remove(context);
        }





        public Task Receive(NetContext context, object message)
        {
            RpcMessage rpcmsg = (RpcMessage)message;

            if (rpcmsg.Type != 2000000003u && Authentication != AuthenticationType.Security)
            {
                RpcMessage resp = new RpcMessage();
                resp.Identifier = rpcmsg.Identifier;
                Error error = new Error();
                error.ErrorCode = RpcException.INVALID_CONNECTION;
                error.ErrorMessage = "Invalid connection!";
                resp.Body = error;
                context.GetLoger(LogLevel.Warring)?.Write(context, "gpRPCGatewaySession", "Invoke", "Invalid connection");
                NetContext?.Send(resp);
                NetContext?.Dispose();
                return Task.CompletedTask;
            }
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
                    context.GetLoger(LogLevel.Debug)?.Write(context, "gpRPCGatewaySession", "Login", "Success");
                }
                else
                {
                    Error error = new Error();
                    error.ErrorCode = RpcException.INVALID_NAME_OR_PASSWORD;
                    error.ErrorMessage = "Invalid user name or password!";
                    resp.Body = error;
                    context.GetLoger(LogLevel.Warring)?.Write(context, "gpRPCGatewaySession", "Login", error.ErrorMessage);
                }

                NetContext?.Send(resp);
                return Task.CompletedTask;
            }
            else if (rpcmsg.Type == 2000000004u)
            {
                OnSubscribe(rpcmsg, (SubscribeReq)rpcmsg.Body);
            }
            else
            {
                if (User == null || !User.Check(rpcmsg.Type))
                {
                    RpcMessage resp = new RpcMessage();
                    resp.Identifier = rpcmsg.Identifier;
                    Error error = new Error();
                    if (User == null)
                    {
                        error.ErrorCode = RpcException.INVALID_CONNECTION;
                        error.ErrorMessage = "Invalid connection!";
                        NetContext?.GetLoger(LogLevel.Warring)?.Write(context, "gpRPCGatewaySession", "Invoke", "Invalid connection");
                    }
                    else
                    {
                        error.ErrorCode = RpcException.PERMISSION_UNAVAILABLE;
                        error.ErrorMessage = "Invalid connection!";
                        NetContext?.GetLoger(LogLevel.Warring)?.Write(context, "gpRPCGatewaySession", "Invoke", "Permission unavailable");
                    }
                    resp.Body = error;
                    NetContext?.Send(resp);

                    NetContext?.Dispose();
                    return Task.CompletedTask;
                }
                if (rpcmsg.Identifier < UInt32.MaxValue)//推送消息
                {
                    var gatewayID = Server.SafeIdGenerator.GetNextId();
                    var subContext = Server.MessageLoadBalancerTable.GetMessageLoadBalancer(rpcmsg.Type).GetContext();
                    if (subContext != null && !subContext.Context.Disposed)
                    {
                        rpcmsg.Identifier = rpcmsg.Identifier | ((UInt64)gatewayID << 32);//增量网关处理ID
                        Server.SubcribeReplyTable.SetReplyContext(rpcmsg.Identifier, context);
                        subContext.Context.Send(rpcmsg);
                        context.GetLoger(LogLevel.Debug)?.Write(context, "gpRPCGatewaySession", $"✉ Publish to {subContext.Context.RemotePoint.ToString()}", $"{rpcmsg.Body.GetType().Name}");
                    }
                    else
                    {
                        RpcMessage resp = new RpcMessage();
                        resp.Identifier = rpcmsg.Identifier;
                        Error error = new Error();
                        error.ErrorCode = RpcException.SERVICE_UNAVAILABLE;
                        error.ErrorMessage = "Service unavailable";
                        resp.Body = error;
                        context.Send(resp);
                        context.GetLoger(LogLevel.Warring)?.Write(context, "gpRPCGatewaySession", "Publish", $"{rpcmsg.Body.GetType().Name} message publish error service unavailable!");
                    }
                }
                else //订阅响应
                {
                    var id = rpcmsg.Identifier;
                    var netContext = Server.SubcribeReplyTable.GetReplyContext(id);
                    id = (id << 32) >> 32;//减去网关增量
                    rpcmsg.Identifier = id;
                    if (netContext != null)
                    {
                        netContext.Send(rpcmsg);
                        context.GetLoger(LogLevel.Debug)?.Write(context, "gpRPCGatewaySession", $"✉ Reply to {netContext.RemotePoint.ToString()}", $"✉ {rpcmsg.Body.GetType().Name}");
                    }
                }

            }
            return Task.CompletedTask;
        }



        private void OnSubscribe(RpcMessage rpcmsg, SubscribeReq req)
        {
            RpcMessage resp = new RpcMessage();
            resp.Identifier = rpcmsg.Identifier;
            try
            {
                if (User == null)
                    throw new RpcException("Invalid connection");
                foreach (var item in req.Items)
                {
                    if (!User.Check(item))
                    {
                        throw new RpcException($"Subscribe {item} permission unavailable!");
                    }

                }
                foreach (var item in req.Items)
                {
                    Server.MessageLoadBalancerTable.Add(item, NetContext);
                    NetContext.GetLoger(LogLevel.Debug)?.Write(NetContext, "gpRPCGatewaySession", "Subscribe", $"{item}");
                }
                resp.Body = new Success();
            }
            catch (Exception e_)
            {
                NetContext.GetLoger(LogLevel.Warring)?.WriteException(NetContext, "gpRPCGatewaySession", "Subscribe", e_);
                Error error = new Error();
                error.ErrorCode = RpcException.SUBSCRIBER_ERROR;
                error.ErrorMessage = e_.Message;
                resp.Body = error;
            }
            NetContext?.Send(resp);
        }

        public void ReceiveCompleted(int bytes)
        {

        }

        public void SendCompleted(int bytes)
        {

        }
    }
}
