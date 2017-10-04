using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Eksponent.Dewd
{
    /// <summary>
    /// Various helper methods for locating and instantiating types.
    /// </summary>
    public static class TypeInstantiater
    {
        public static Type SearchType(string typeValue, Type defaultType, string[] implicitNamespaces)
        {
            if (typeValue.Length == 0)
                return defaultType;

            Type type = Type.GetType(typeValue);
            if (type != null)
                return type;

            foreach (string implicitNamespace in implicitNamespaces)
            {
                type = Type.GetType(String.Format(implicitNamespace, typeValue));
                if (type != null)
                    return type;
            }

            return null;
        }

        public static T GetInstance<T>(string typeName, Type defaultType, string[] searchPaths, params object[] parameters) 
        {            
            Type type = SearchType(typeName, defaultType, searchPaths);
            if(type==null)
                throw new TypeException("Unable to locate type '{0}'.", typeName);

            return (T)Activator.CreateInstance(type, parameters);
        }

        public static T GetInstance<T>(string typeName, Type defaultType, params object[] parameters)
        {
            return GetInstance<T>(typeName, defaultType, new string[0], parameters);
        }
    }

    public class TypeException : FormattedException
    {
        public TypeException(string message, params object[] parameters)
            : base(message, parameters) 
        {
        }
    }
}