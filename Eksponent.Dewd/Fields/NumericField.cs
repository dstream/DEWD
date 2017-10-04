using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Eksponent.Dewd.Editors;

namespace Eksponent.Dewd.Fields
{
    public class NumericField : Field
    {
        public NumericField(XElement fieldElement, IEditor editor)
            : base(fieldElement, editor)
        {
        }

        public override void ProcessValueBeforeSave(ref object value)
        {
            // todo: support most common numeric types
            decimal decVal = 0;
            if (Decimal.TryParse(String.Format("{0}", value), System.Globalization.NumberStyles.Any, Culture, out decVal))
                value = decVal;

            base.ProcessValueBeforeSave(ref value);
        }
    }
}