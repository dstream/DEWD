using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Eksponent.Dewd.Repositories;

namespace Eksponent.Dewd.Repositories.Table
{
    public class TestEditor : TableEditor
    {
        public TestEditor(XElement editorElement, IRepository repos) : base(editorElement, repos)
        {
            BeforeSave += (obj, args) =>
            {
                args.Result = new Editors.SaveResult() { 
                    ErrorMessage = "This editor will NOT do your bidding.", 
                    Success = false 
                };
                args.Cancel = true;
            };
        }
    }
}