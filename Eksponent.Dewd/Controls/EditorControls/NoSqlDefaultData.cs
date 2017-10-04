using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.datatype;

namespace Eksponent.Dewd.Controls.EditorControls
{
    public class NoSqlDefaultData : DefaultData
    {
        private object _value;

        public NoSqlDefaultData(BaseDataType dt)
            : base(dt)
        {
        }

        public override object Value
        {
            get { return _value; }
            set { _value = value; }
        }
    }
}