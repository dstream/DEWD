using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Eksponent.Dewd
{
    /// <summary>
    /// Simple helper class for holding objects during requests.
    /// </summary>
    public static class RequestTemp
    {
        private const string BASE_KEY = "Dewd.{0}"; 

        public static void Put(string key, object value)
        {
            if (HttpContext.Current == null)
                return;
            HttpContext.Current.Items[String.Format(BASE_KEY, key)] = value;
        }

        public static object Get(string key)
        {
            if (HttpContext.Current == null)
                return null;
            return HttpContext.Current.Items[String.Format(BASE_KEY, key)];
        }

        public static T Get<T>(string key) where T : class
        {
            return Get(key) as T;
        }
    }
}