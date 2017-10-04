using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Eksponent.Dewd
{
    public interface IRequiresPageAccess
    {
        void PagePreInitCallBack(Page page);
    }
}