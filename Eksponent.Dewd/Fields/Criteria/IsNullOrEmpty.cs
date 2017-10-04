using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Eksponent.Dewd.Extensions;

namespace Eksponent.Dewd.Fields.Criteria
{
    public class IsNullOrEmpty : IsNull
    {
        public IsNullOrEmpty(XElement element) : base(element) { }

        public override bool IsTrue(object value)
        {
            if (base.IsTrue(value))
                return true; // is null

            string strValue = String.Format("{0}", value);
            if (CriterionElement.GetAttribute<bool>("trim"))
                strValue = strValue.Trim();

            return (strValue.Length == 0);
        }
    }
}