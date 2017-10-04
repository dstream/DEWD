using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Eksponent.Dewd.Repositories;
using Eksponent.Dewd.Views;
using System.Xml.Linq;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace Eksponent.Dewd.Controls.View
{
    /// <summary>
    /// Snippets allows a ASP.NET control declaration to be parsed and used as a view control.
    /// </summary>
    public class Snippet : WebControl, IViewControl
    {
        public virtual IRepository Repository { get; set; }
        public virtual IView View { get; set; }
        private IViewControl subControl; 

        public Snippet()
        {
        }

        protected override void OnInit(EventArgs e)
        {
            XElement snippetElement = ViewElement.Element("snippet");
            if (snippetElement == null)
                throw new ApplicationException("Snippet control requires snippet-node as child of view-node.");

            var snippet = snippetElement.Value;
            var control = Page.ParseControl(snippet);
            subControl = (IViewControl)control.Controls.OfType<IViewControl>().First();
            subControl.View = View;
            subControl.Repository = Repository;
            subControl.EnableEdit = EnableEdit;
            Controls.Add((Control)subControl);

            base.OnInit(e);
        }

        public XElement ViewElement
        {
            get
            {
                if (View is IXConfigurable)
                    return ((IXConfigurable)View).ConfigurationElement;
                return null;
            }
        } 

        public virtual bool EnableEdit
        {
            get { return subControl.EnableEdit; }
            set { subControl.EnableEdit = value; }
        }

        public object DataSource
        {
            get
            {
                return subControl.DataSource;
            }
            set
            {
                subControl.DataSource = value;
            }
        }

        public new void DataBind()
        {
            subControl.DataBind();
        }
    }
}