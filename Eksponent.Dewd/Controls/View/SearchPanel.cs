using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace Eksponent.Dewd.Controls.View
{
    public class SearchPanel : WebControl
    {
        protected TextBox searchBox;
        private LinkButton searchButton;
        private LinkButton clearButton;
        protected bool postbackHasTriggeredSearch = false;

        public virtual bool IsSearching
        {
            get { return postbackHasTriggeredSearch || Text.Length != 0; }
        }

        public virtual string Text
        {
            get { return searchBox == null ? "" : searchBox.Text; }
        }

        public virtual void AddSearchInputControls()
        {
            Controls.Add(searchBox = new TextBox() { AutoPostBack = true });
        }

        public virtual void OnSearch(object sender, EventArgs args)
        {
            postbackHasTriggeredSearch = true;
        }

        public virtual void OnClear(object sender, EventArgs args)
        {
            if (searchBox != null) searchBox.Text = "";
        }

        protected override void OnInit(EventArgs e)
        {
            Controls.Add(new LiteralControl("Search:&nbsp;"));
            AddSearchInputControls();
            
            Controls.Add(searchButton = new LinkButton() { Text = "search" });
            searchButton.Style.Value = "margin-left:5px;margin-right:5px";
            searchButton.Click += OnSearch;

            Controls.Add(clearButton = new LinkButton() { Text = "clear" });
            clearButton.Click += OnClear;
            
            base.OnInit(e);
        }

    }
}