using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Eksponent.Dewd.Extensions
{
    public static class XElementExtensions
    {
        public static string GetAttribute(this XElement element, string name)
        {
            return GetAttribute(element, name, "");
        }

        public static string GetAttribute(this XElement element, string name, string defaultValue)
        {
            var attr = element.Attribute(name);
            if (attr == null) 
                return defaultValue;
            return attr.Value;
        }

        public static T GetAttribute<T>(this XElement element, string name)
        {
            return GetAttribute<T>(element, name, default(T));
        }

        public static T GetAttribute<T>(this XElement element, string name, T defaultValue) {
            string strValue = GetAttribute(element, name);
            if (strValue.Length == 0)
                return defaultValue;

            try
            {
                if (typeof(T) == typeof(Boolean))
                    return (T)(object)Boolean.Parse(strValue);
                if (typeof(T) == typeof(Int32))
                    return (T)(object)Int32.Parse(strValue);
                if (typeof(T) == typeof(Type))
                    return (T)(object)Type.GetType(strValue);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    String.Format("Unable to convert '{0}' in '{1}' to type: {2}", strValue, name, typeof(T)), ex);
            }
            throw new NotSupportedException(String.Format("Unknown type: {0}", typeof(T)));
        }
    }
}