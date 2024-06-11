using BeetleX.Light;
using BeetleX.Light.Clients;
using BeetleX.Light.Logs;
using BeetleX.Light.Protocols;
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

        public void Receive(NetContext context, object message)
        {
            OnReceiveMessage(message);
        }

        protected virtual async void OnReceiveMessage(object message)
        {


            var method = MessageSessionHandlers.Default.GetMethod(message.GetType());
            if (method != null)
            {
                try
                {
                    NetContext.GetLoger(LogLevel.Debug)?.Write(NetContext, "gpRPCSession", "Call", $"{message.GetType().Name} starting");
                    Task task = (Task)method.Method.Invoke(method.Service, new object[] { message });
                    await task;
                    var result = method.ResultProperty.GetValue(task);
                    IIdentifier req = (IIdentifier)message;
                    IIdentifier resp = (IIdentifier)result;
                    resp.Identifier = req.Identifier;
                    NetContext?.Send(resp);
                    NetContext.GetLoger(LogLevel.Debug)?.Write(NetContext, "gpRPCSession", "Call", $"{message.GetType().Name} returned");

                }
                catch (Exception e_)
                {
                    NetContext?.GetLoger(LogLevel.Error)?.WriteException(NetContext, "gpRPCSession", message.GetType().Name, e_);
                }

            }
            else
            {
                NetContext?.GetLoger(LogLevel.Warring)?.Write(NetContext, "gpRPCSession", "Call", $"{message.GetType().Name} handler not found!");
            }

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
                    if (resp.GetInterface("Google.Protobuf.IMessage") == null || req.GetInterface("Google.Protobuf.IMessage") == null ||
                        resp.GetInterface("BeetleX.Light.Protocols.IIdentifier") == null || req.GetInterface("BeetleX.Light.Protocols.IIdentifier") == null
                        )
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
