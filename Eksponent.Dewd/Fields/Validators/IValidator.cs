using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Eksponent.Dewd.Fields.Validators
{
    public interface IValidator
    {
        string ErrorText { get; }
        bool IsValid(object value);
    }
}