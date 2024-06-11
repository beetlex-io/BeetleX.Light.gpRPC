using BeetleX.Light.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Ligth.gpRPC
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
                var reqs = RpcClient.Request((IIdentifier)args[0]);
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
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
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
                ActionHandler action = new ActionHandler(method);
                mHandlers[req] = action;

            }
        }
    }
}
