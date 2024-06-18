using BeetleX.Light;
using BeetleX.Light.Clients;
using BeetleX.Light.Logs;
using BeetleX.Light.Protocols;
using BeetleX.Ligth.gpRPC.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Ligth.gpRPC
{
    public class RpcSession : ISession
    {
        public RpcSession()
        {

        }
        public string Name { get; set; }

        public AuthenticationType Authentication { get; set; } = AuthenticationType.None;

        public NetContext NetContext { get; private set; }

        public virtual void Connected(NetContext context)
        {
            NetContext = context;
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
            return OnReceiveMessage(message);
        }

        protected virtual async Task OnReceiveMessage(object message)
        {

            RpcMessage req = (RpcMessage)message;
            RpcMessage resp = new RpcMessage();
            resp.Identifier = req.Identifier;
            var method = MessageSessionHandlers.Default.GetMethod(req.Body.GetType());
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
                    error.ErrorMessage = e_.Message;
                    error.StackTrace = e_.StackTrace;
                    resp.Body = error;
                    NetContext?.GetLoger(LogLevel.Error)?.Write(NetContext, "gpRPCSession", "InvokeError", $"{req.Body.GetType().Name} invok error {e_.Message} {e_.StackTrace}!");
                }

            }
            else
            {
                Error error = new Error();
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

        internal class MessageSessionHandlers
        {
            private Dictionary<Type, MethodInvokeHandler> _methods = new Dictionary<Type, MethodInvokeHandler>();

            private static readonly MessageSessionHandlers _default = new MessageSessionHandlers();

            public static MessageSessionHandlers Default => _default;

            public MessageSessionHandlers()
            {

            }
            public MethodInvokeHandler GetMethod(Type msgType)
            {
                _methods.TryGetValue(msgType, out var method);
                return method;
            }

            public void Register(Type type, INetServer server)
            {
                var service = Activator.CreateInstance(type);
                foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (string.Compare("Equals", method.Name, true) == 0
            || string.Compare("GetHashCode", method.Name, true) == 0
            || string.Compare("GetType", method.Name, true) == 0
            || string.Compare("ToString", method.Name, true) == 0 || method.Name.IndexOf("set_") >= 0
            || method.Name.IndexOf("get_") >= 0 || method.GetParameters().Length != 1 ||
            method.ReturnType.BaseType != typeof(Task))
                        continue;
                    var req = method.GetParameters()[0].ParameterType;
                    var resp = method.ReturnType.GetGenericArguments()[0];
                    if (resp.GetInterface("Google.Protobuf.IMessage") == null || req.GetInterface("Google.Protobuf.IMessage") == null)
                        continue;
                    server.GetLoger(LogLevel.Info)?.Write((EndPoint)null, "gpRPC", "ObjectMapping", $"{req.Name} mapping to {type.Name}.{method.Name}");
                    var handler = new MethodInvokeHandler(method);
                    handler.Service = service;
                    _methods[req] = handler;
                }
            }

        }

        internal class MethodInvokeHandler
        {
            public MethodInvokeHandler(MethodInfo method)
            {

                ResultProperty = method.ReturnType.GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
                Method = method;
            }

            public MethodInfo Method { get; set; }

            public PropertyInfo ResultProperty
            {
                get; set;
            }

            public object Service { get; set; }
        }


    }
}
