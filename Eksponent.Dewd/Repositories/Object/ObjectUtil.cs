using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Eksponent.Dewd.Fields;
using System.Reflection;
using Eksponent.Dewd.Extensions;

namespace Eksponent.Dewd.Repositories.Object
{
    public static class ObjectUtil
    {
        private static PropertyInfo GetProp(object obj, string name)
        {
            var prop = obj.GetType().GetProperty(name);
            if (prop == null)
                throw new FormattedException("Property {0} not found on object of type {1}", name, obj.GetType());
            return prop;
        }

        public static Dictionary<string, object> GetPropertyValues(IEnumerable<IField> fields, object obj)
        {
            var values = new Dictionary<string, object>();
            foreach (IField field in fields.Where(f => f is IHasSourceField))
            {
                var source = ((IHasSourceField)field).SourceField;
                values.Add(field.UniqueID, GetProp(obj, source).GetValue(obj, null));
            }
            return values;
        }

        public static void SetPropertyValues(object obj, Dictionary<string, object> values, IEnumerable<IField> fields)
        {
            foreach (IField field in fields.Where(f => f is IHasSourceField))
            {
                var source = ((IHasSourceField)field).SourceField;
                var value = values[field.UniqueID];

                // allow a type conversion to occur.. hmm, why not handled by ProcessValueBeforeSave?
                var targetType = field.Xml.GetAttribute("targetType");
                if (targetType.Length != 0)
                {
                    var type = TypeInstantiater.SearchType(targetType, typeof(string), new string[] { "System.{0}" });
                    value = Convert.ChangeType(value, type);
                }

                GetProp(obj, source).SetValue(obj, value, null);
            }
        }
    }
}