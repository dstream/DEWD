using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Eksponent.Dewd.Editors;

namespace Eksponent.Dewd.Repositories.Object
{
    public abstract class ObjectEditor : Editor
    {
        public ObjectEditor(XElement element, IRepository repository)
            : base(element, repository)
        {
        }
    }
}