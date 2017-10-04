using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Eksponent.Dewd.Extensions;
using umbraco.DataLayer;
using umbraco;
using Eksponent.Dewd.Controls;
using umbraco.uicontrols;
using umbraco.cms.businesslogic.datatype;
using Eksponent.Dewd;
using Eksponent.Dewd.Repositories;
using System.Xml.Linq;
using umbraco.BasePages;

namespace Eksponent.Dewd.Pages
{
    public partial class Edit : UmbracoEnsuredPage, IEditorPage
    {
        private XElement tableElement;
        private XElement editorElement;
        private Configuration configuration;
        private IRepository repository;
        private Editors.IEditor editor;
        private string baseUrl = Configuration.BaseUrl;
        private RowID id;
        private ContentControlContainer ccc;

        protected override void OnPreInit(EventArgs e)
        {
            id = RowID.Get(Request["id"]);
            repository = Configuration.Current.GetRepository(Request["txe"]);
            editor = repository.Editor;
            Eksponent.Dewd.Context.Repository = repository;
            Eksponent.Dewd.Context.Editor = editor;

            var pageAccessView = (editor as IRequiresPageAccess);
            if (pageAccessView != null)
                pageAccessView.PagePreInitCallBack(this);

            base.OnPreInit(e);
        }

        public event EventHandler<EventArgs> ContentControlContainerCreated;

        public ContentControlContainer ContentControlContainer
        {
            get { return ccc; }
        }

        protected override void OnInit(EventArgs e)
        {
            var controlsDefs = editor.GetContentControlDefinitions(id);

            bool skipBubble = false;
            ccc = new ContentControlContainer();

            Holder.Controls.Add(ccc);

            // add events
            ccc.Save += (obj, args) =>
            {
                var result = editor.Save(ref id, args.Values);
                if (result.Success)
                {
                    if (args.Return)
                        RedirectToView("saved");
                    else
                        Response.Redirect(baseUrl + "Edit.aspx?txe=" + Server.UrlPathEncode(Request["txe"] + "&id=" + id.Value + "&feedback=saved"));
                }
                else
                {
                    this.speechBubble(BasePage.speechBubbleIcon.error, "Error", "Changes was not saved.");
                    ccc.DisplayError("Unable to save:", result.ErrorMessage);
                    skipBubble = true;
                }
            };
            ccc.Delete += (obj, args) =>
            {
                editor.Delete(id);
                RedirectToView("deleted");
            };
            ccc.Cancel += (obj, args) =>
            {
                RedirectToView("");
            };

            // add standard buttons
            ccc.MenuButtons.Add(new ContentControlContainer.SaveButton(ccc, "saveback", baseUrl + "SaveAndReturn.png"));
            ccc.MenuButtons.Add(new ContentControlContainer.SaveButton(ccc, "save", GlobalSettings.Path + "/images/editor/save.gif"));
            if (id != null)
                ccc.MenuButtons.Add(new ContentControlContainer.DeleteButton(ccc));
            ccc.MenuButtons.Add(new ContentControlContainer.CancelButton(ccc));

            // add custom editor buttons
            var editorWithButtons = (editor as IRequiresMenuButtons);
            if (editorWithButtons != null)
                editorWithButtons.AddMenuButtons();

            if (ContentControlContainerCreated != null)
                ContentControlContainerCreated(this, new EventArgs());
            ccc.AddControls(controlsDefs);

            if (!skipBubble)
                UiHelper.HandleSpeechFeedback(this);

            base.OnInit(e);
        }

        private void ToView(string parameters)
        {
            Response.Redirect(baseUrl + "View.aspx?txe=" + Server.UrlPathEncode(Request["txe"]) + parameters);
        }

        public void RedirectToView(BasePage.speechBubbleIcon icon, string title, string text)
        {
            ToView("&feedback=" + icon +
                "&feedbackTitle=" + Server.UrlPathEncode(title) +
                "&feedbackText=" + Server.UrlPathEncode(text));
        }

        public void RedirectToView(string predefinedFeedback)
        {
            ToView("&feedback=" + predefinedFeedback);
        }
    }
}