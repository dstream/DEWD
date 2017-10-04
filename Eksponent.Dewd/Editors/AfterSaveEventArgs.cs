using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Eksponent.Dewd.Editors
{
    public class AfterSaveEventArgs : RowEventArgs
    {
        public Dictionary<string, object> Values { get; set; }
    }
}