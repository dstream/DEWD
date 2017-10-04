using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Eksponent.Dewd.Extensions;
using System.Text.RegularExpressions;

namespace Eksponent.Dewd.Fields.Validators
{
    public class RegEx : IValidator
    {
        private XElement validatorElement;

        public RegEx(XElement validatorElement)
        {
            this.validatorElement = validatorElement;
        }

        public virtual string ErrorText
        {
            get {
                return validatorElement.GetAttribute("errorText", "Field {0} has an invalid value.");
            }
        }

        public virtual bool IsValid(object value)
        {
            return (new Criteria.RegEx(validatorElement)).IsTrue(value);
        }
    }
}