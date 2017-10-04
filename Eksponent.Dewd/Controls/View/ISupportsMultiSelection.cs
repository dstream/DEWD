using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Eksponent.Dewd.Controls.View
{
    public interface ISupportsMultiSelection
    {
        bool EnableMultiSelection { set; get; }
        RowID[] GetSelectedRowIDs();
    }
}