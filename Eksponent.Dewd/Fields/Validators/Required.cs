using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Eksponent.Dewd.Extensions;

namespace Eksponent.Dewd.Fields.Validators
{
    public class Required : IValidator
    {
        private XElement validatorElement;

        public Required(XElement validatorElement)
        {
            this.validatorElement = validatorElement;
        }

        public virtual string ErrorText
        {
            get {
                return validatorElement.GetAttribute("errorText", "Required field {0} is missing a value.");
            }
        }

        public virtual bool IsValid(object value)
        {
            if (!validatorElement.GetAttribute<bool>("required"))
                return true;

            string strValue = String.Format("{0}", value);
            if (validatorElement.GetAttribute<bool>("trim"))
                strValue = strValue.Trim();

            return strValue.Length != 0;
        }
    }
}