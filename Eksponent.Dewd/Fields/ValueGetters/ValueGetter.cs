using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Eksponent.Dewd.Extensions;

namespace Eksponent.Dewd.Fields.ValueGetters
{
    public static class ValueGetter
    {
        public static object GetFromValueGetter(XElement getterElement)
        {
            var getter = TypeInstantiater.GetInstance<ValueGetters.IValueGetter>(
                getterElement.GetAttribute("type"),
                typeof(ValueGetters.Standard),
                new string[] { "Eksponent.Dewd.Fields.ValueGetters.{0},Eksponent.Dewd" });
            return getter.GetValue(getterElement);
        }
    }
}