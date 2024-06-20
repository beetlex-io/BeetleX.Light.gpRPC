using BeetleX.Light.Protocols;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Light.gpRPC
{
    public class RpcInterfaceProxy : DispatchProxy
    {


        internal RpcClient RpcClient { get; set; }

        internal Type Type { get; set; }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            var req = args[0].GetType();
            if (mHandlers.TryGetValue(req, out var handler))
            {
                var resp = handler.GetCompletionSource();
                var reqs = RpcClient.Request((IMessage)args[0]);
                resp.Wait(reqs);
                return resp.GetTask();
            }
            else
            {
                throw new NotImplementedException(targetMethod.Name);
            }
        }


        private Dictionary<Type, ActionHandler> mHandlers = new Dictionary<Type, ActionHandler>();

        private Dictionary<string, string> mHeader = new Dictionary<string, string>();

        internal void InitHandlers()
        {
            Type type = Type;
            Type gtask = Type.GetType("System.Threading.Tasks.Task`1");
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (string.Compare("Equals", method.Name, true) == 0
              || string.Compare("GetHashCode", method.Name, true) == 0
              || string.Compare("GetType", method.Name, true) == 0
              || string.Compare("ToString", method.Name, true) == 0 || method.Name.IndexOf("set_") >= 0
              || method.Name.IndexOf("get_") >= 0 || method.GetParameters().Length != 1 ||
              (method.ReturnType.Name != "Task`1" && method.ReturnType != typeof(Task)))
                    continue;
                var req = method.GetParameters()[0].ParameterType;
                Type resp = null;
                if (method.ReturnType.IsGenericType)
                {
                    resp = method.ReturnType.GetGenericArguments()[0];
                }
                if (req.GetInterface("Google.Protobuf.IMessage") != null && (method.ReturnType == typeof(Task) || resp?.GetInterface("Google.Protobuf.IMessage") != null))
                {
                    ActionHandler action = new ActionHandler(method);
                    mHandlers[req] = action;
                }

            }
        }
    }
}
