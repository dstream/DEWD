using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Eksponent.Dewd.Repositories.Umbraco
{
    public class UmbracoRepository : Repository
    {
        public UmbracoRepository(XElement element)
            : base(element)
        {
        }

        protected override Type DefaultEditorType
        {
            get { return typeof(UmbracoEditor); }
        }

        protected override Type DefaultViewType
        {
            get { return typeof(UmbracoView); }
        }
    }
}