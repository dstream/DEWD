using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Eksponent.Dewd.Repositories.Object
{
    public class ObjectRepository : Repository
    {
        public ObjectRepository(XElement element)
            : base(element)
        {
        }

        protected override Type DefaultEditorType
        {
            get { return typeof(ObjectEditor); }
        }

        protected override Type DefaultViewType
        {
            get { return typeof(ObjectView); }
        }
    }
}