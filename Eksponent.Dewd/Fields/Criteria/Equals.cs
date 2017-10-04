using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Eksponent.Dewd.Extensions;

namespace Eksponent.Dewd.Fields.Criteria
{
    public class Equals : Criterion
    {
        public Equals(XElement element) : base(element) { }

        public object GetComparisonValue()
        {
            object compareValue;
            var valueGetter = CriterionElement.Element("value");
            if (valueGetter != null)
                compareValue = ValueGetters.ValueGetter.GetFromValueGetter(valueGetter);
            else
                compareValue = CriterionElement.Attribute("value");
            return compareValue;
        }

        public override bool IsTrue(object value)
        {
            var compareValue = GetComparisonValue();
            if (compareValue == null && value != null)
                return false;

            //Trace.Warn("CompareVlaue: {0}, Value {1}, Equals: {2}", compareValue, value, (value.Equals(compareValue)));

            return (compareValue.Equals(value));
        }
    }
}