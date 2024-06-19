using BeetleX.Light.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Light.gpRPC
{
    internal class MessageMethodHandlers
    {
        private Dictionary<Type, MethodInvokeHandler> _methods = new Dictionary<Type, MethodInvokeHandler>();

        private static readonly MessageMethodHandlers _default = new MessageMethodHandlers();

        public static MessageMethodHandlers Default => _default;

        public MessageMethodHandlers()
        {

        }
        public MethodInvokeHandler GetMethod(Type msgType)
        {
            _methods.TryGetValue(msgType, out var method);
            return method;
        }

        public Func<Type, object> CreateInstanceHandler { get; set; }

        private object CreateInstance(Type type)
        {
            if (CreateInstanceHandler != null)
                return CreateInstanceHandler(type);
            return Activator.CreateInstance(type);
        }

        public void Register(Type type, IGetLogHandler loger)
        {
            var service = CreateInstance(type);
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
                loger.GetLoger(LogLevel.Info)?.Write((EndPoint)null, "gpRPC", "ObjectMapping", $"{req.Name} mapping to {type.Name}.{method.Name}");
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
