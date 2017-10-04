using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;

namespace Eksponent.Dewd
{
    /// <summary>
    /// Allows a method to be called by identifying it using the form: MyNameSpace.MyType.MyMethod,MyAssembly.
    /// </summary>
    public class MethodCaller
    {
        public Type Type { get; private set; }
        public string MethodName { get; private set; }

        public MethodCaller(string fullName)
        {
            string fullMethodName = fullName.Split(',')[0]; // this name includes type name
            string assemblyName = fullName.Split(',')[1];
            string typeName = fullMethodName.Substring(0, fullMethodName.LastIndexOf('.'));
            MethodName = fullMethodName.Substring(fullMethodName.LastIndexOf('.') + 1);

            // look up type
            string fullTypeName = typeName + "," + assemblyName;
            Type = Type.GetType(fullTypeName);
            if (Type == null)
                throw new FormattedException("Type '{0}' not found, which is used in method name '{1}'.", fullTypeName, fullName);
        }

        private MethodInfo GetMethodInfo(params object[] args)
        {
            // look up method
            List<Type> parameterTypes = new List<Type>();
            foreach (object obj in args)
                parameterTypes.Add(obj.GetType());
            MethodInfo method =
                Type.GetMethod(MethodName, parameterTypes.ToArray());
            if (method == null)
                throw new FormattedException("Method '{0}' not found on type '{1}' or parameters are mismatched.", MethodName, Type);
            return method;
        }

        public T Invoke<T>(params object[] args)
        {
            return (T)(GetMethodInfo(args).Invoke(null, args));
        }

        public void Invoke(params object[] args)
        {
            GetMethodInfo(args).Invoke(null, args);
        }

        public static T Invoke<T>(string fullName, params object[] args)
        {
            return (new MethodCaller(fullName)).Invoke<T>(args);
        }

        public static void Invoke(string fullName, params object[] args)
        {
            (new MethodCaller(fullName)).Invoke(args);
        }
    }
}