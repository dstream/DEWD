using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Eksponent.Dewd.Fields.Criteria
{
    public interface ICriterion
    {
        bool IsTrue(object value);
    }
}