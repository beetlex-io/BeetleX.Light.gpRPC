using BeetleX.Light.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Light.gpRPC
{
    public class ActionHandler
    {
        public ActionHandler(MethodInfo method)
        {
            this.MethodInfo = method;

            if (method.ReturnType.IsGenericType)
            {
                ResultType = method.ReturnType.GetGenericArguments()[0];
                PropertyInfo pi = method.ReturnType.GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
            }
            else
            {
                ResultType = typeof(object);
            }

        }

        public MethodInfo MethodInfo { get; set; }


        public Type ResultType { get; set; }


        private Type mCompletionSourceType;

        public object Execute(object controller, params object[] parameters)
        {
            return MethodInfo.Invoke(controller, parameters);
        }

        internal IAnyCompletionSource GetCompletionSource()
        {
            if (mCompletionSourceType == null)
            {
                Type gtype = typeof(AnyCompletionSource<>);
                mCompletionSourceType = gtype.MakeGenericType(ResultType);

            }
            return (IAnyCompletionSource)Activator.CreateInstance(mCompletionSourceType);
        }
    }
}
