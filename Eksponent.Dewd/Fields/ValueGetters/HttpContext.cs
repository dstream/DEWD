using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Eksponent.Dewd.Extensions;
using System.Web.UI;

namespace Eksponent.Dewd.Fields.ValueGetters
{
    public class HttpContext : Standard
    {
        public override object CreateValueFromElement(System.Xml.Linq.XElement valueElement)
        {
            var collection = valueElement.GetAttribute("collection");
            var context = System.Web.HttpContext.Current;
            var key = valueElement.GetAttribute("key");
            switch (collection.ToLower())
            {
                case "request": return context.Request[key];
                case "request-browser": return DataBinder.Eval(context.Request.Browser, key);
                case "request-uri": return DataBinder.Eval(context.Request.Url, key);
                case "session": return context.Session[key];
                case "items": return context.Items[key];
                case "profile": return context.Profile[key];
                case "cache": return context.Cache[key];
                case "user-identity": return DataBinder.Eval(context.User.Identity, key);
                default:
                    return DataBinder.Eval(context, key);
            }            
        }
    }
}