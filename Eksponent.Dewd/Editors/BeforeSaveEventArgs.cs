using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Eksponent.Dewd.Editors
{
    public class BeforeSaveEventArgs : RowEventArgs
    {
        public Dictionary<string, object> Values { get; set; }
        public bool Cancel { get; set; }
        public SaveResult Result { get; set; }
    }
}