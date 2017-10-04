using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Eksponent.Dewd.Extensions;

namespace Eksponent.Dewd.Fields.ValueGetters
{
    public class Standard : IValueGetter
    {
        public virtual object CreateValueFromElement(XElement valueElement)
        {
            // check for predefined getter
            var getName = valueElement.GetAttribute("get").ToLower();
            switch (getName)
            {
                case "null":
                    return null;
                case "dbnull":
                    return DBNull.Value;
                case "today":
                    return DateTime.Today;
                case "now":
                    return DateTime.Now;
                case "now-utc":
                    return DateTime.UtcNow;
                case "guid":
                    return Guid.NewGuid();
            }

            // get inner text (or value-subelement with text)
            return valueElement.HasElements ? valueElement.Element("value").Value : valueElement.Value;            
        }

        public object GetValue(XElement valueElement)
        {
            var val = CreateValueFromElement(valueElement);

            // check for another target type to return
            var targetType = valueElement.GetAttribute("targetType");
            if (targetType.Length == 0)
                return val;

            //var type = Configuration.GetCustomType(targetType, typeof(string), new string[] { "System.{0}" });
            var type = TypeInstantiater.SearchType(targetType, typeof(string), new string[] { "System.{0}" });
            if (type == null)
                throw new FormattedException("Target type not found '{0}' in {1}", targetType, valueElement);
            return Convert.ChangeType(val, type);
        }
    }
}