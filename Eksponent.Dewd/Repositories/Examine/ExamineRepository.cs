using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Eksponent.Dewd.Repositories.Examine
{
    public class ExamineRepository : Repository
    {
        public ExamineRepository(XElement element)
            : base(element)
        {
        }

        protected override Type DefaultEditorType
        {
            get { return null;/* typeof(ExamineEditor);*/ }
        }

        protected override Type DefaultViewType
        {
            get { return typeof(ExamineView); }
        }
    }
}