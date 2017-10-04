using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Eksponent.Dewd.Editors;

namespace Eksponent.Dewd.Fields
{
    public class RetrieveValueEventArgs : RowEventArgs
    {
        public object Value { get; set; }
    }
}