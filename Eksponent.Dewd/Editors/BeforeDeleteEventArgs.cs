using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Eksponent.Dewd.Editors
{
    public class BeforeDeleteEventArgs : RowEventArgs
    {
        public bool Cancel { get; set; }
    }
}