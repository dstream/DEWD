using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Eksponent.Dewd
{
    public class FormattedException : ApplicationException
    {
        public FormattedException() : base()
        {
        }

        public FormattedException(string message)
            : base(message)
        {
        }

        public FormattedException(string message, params object[] parameters)
            : base(String.Format(message, parameters)) 
        {
        }
    }
}