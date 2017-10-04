using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Eksponent.Dewd.Editors;

namespace Eksponent.Dewd.Fields
{
    public class YesNo : Eksponent.Dewd.Fields.Field
    {
        public YesNo(XElement element, IEditor editor) : base(element, editor) {         
        }

		public override void ProcessValueBeforeSave(ref object value)
		{
			if (value is string)
			{
				switch (((string)value))
				{
					case "1":
						value = true;
						break;

					case "0":
						value = false;
						break;
				}
			}

			base.ProcessValueBeforeSave(ref value);
		}

        public override void ProcessValueBeforeEdit(ref object value)
        {
            if (value is bool)
                value = (bool)value ? "1" : "0";
            base.ProcessValueBeforeEdit(ref value);
        }
    }
}