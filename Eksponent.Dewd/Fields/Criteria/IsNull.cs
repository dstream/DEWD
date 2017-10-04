using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Eksponent.Dewd.Fields.Criteria
{
    public class IsNull : Criterion
    {
        public IsNull(XElement element) : base(element) { }

        public override bool IsTrue(object value)
        {
            return (value == null);
        }
    }
}