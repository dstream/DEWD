using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Eksponent.Dewd.Extensions;

namespace Eksponent.Dewd.Fields.Criteria
{
    public class RegEx : Criterion
    {
        public RegEx(XElement element) : base(element) { }

        public override bool IsTrue(object value)
        {
            string regEx = CriterionElement.GetAttribute("pattern");
            if (regEx.Length == 0)
                throw new ApplicationException("Missing or empty pattern-attribute from validation-node.");

            return Regex.IsMatch(String.Format("{0}", value), regEx);
        }
    }
}