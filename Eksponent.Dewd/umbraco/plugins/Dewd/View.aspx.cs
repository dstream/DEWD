using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BasePages;
using Eksponent.Dewd.Extensions;
using umbraco.DataLayer;
using umbraco;
using Eksponent.Dewd;
using Eksponent.Dewd.Repositories;
using Eksponent.Dewd.Views;
using System.Data;
using Eksponent.Dewd.Controls.View;

namespace Eksponent.Dewd.Pages
{
    public partial class View : UmbracoEnsuredPage
    {
        private IRepository repository;
        private IView view;
        private IViewControl viewControl;
        private string baseUrl = Eksponent.Dewd.Configuration.BaseUrl;

        protected override void OnPreInit(EventArgs e)
        {
            // determine current repository and view
            var configuration = Eksponent.Dewd.Configuration.Current;
            repository = configuration.GetRepository(Request["txe"]);
            Eksponent.Dewd.Context.Repository = repository;

            view = repository.Views.FirstOrDefault(v => v.Name == Request["vxe"]);
            if (view == null)
                view = repository.Views.First();

            Eksponent.Dewd.Context.View = view;

            var pageAccessView = (view as IRequiresPageAccess);
            if (pageAccessView != null)
                pageAccessView.PagePreInitCallBack(this);

            base.OnPreInit(e);
        }

        protected override void OnInit(EventArgs e)
        {
            Page.ClientScript.RegisterClientScriptInclude("DewdDefault",
                this.Page.ClientScript.GetWebResourceUrl(typeof(IRepository), "Eksponent.Dewd.Default.js"));

            // build view dropdown
            var urlPrefix = baseUrl + String.Format("View.aspx?txe={0}&vxe=", Server.UrlPathEncode(Request["txe"]));
            ViewList.Items.AddRange(
                repository.Views.Select(v =>
                    new ListItem()
                    {
                        Text = v.Name,
                        Value = urlPrefix + Server.UrlPathEncode(v.Name),
                        Selected = view.Name == v.Name
                    }).ToArray());
            SelectorHolder.Visible = (ViewList.Items.Count > 1);

            // add control which displays the data list/grid
            viewControl = view.CreateViewControl();
            ViewControlHolder.Controls.Add((Control)viewControl);

            // initialize edit-mode
            viewControl.EnableEdit = !(repository.Editor == null);

            Eksponent.Dewd.UiHelper.HandleSpeechFeedback(this);

            // include legacy stuff in header
            Umbraco40Header.Visible = Util.UmbracoVersion < new Version("4.1.0");

            base.OnInit(e);
        }

        protected void Page_Navigate(object sender, EventArgs e)
        {
            int page = 0;
            Int32.TryParse(PageCurrent.Text, out page);
            PageCurrent.Text = (page + (((Control)sender).ID == "PagePrevious" ? -1 : 1)).ToString();
        }

        private object GetData(IView view)
        {
            object data;

            var pageableView = (view as ISupportsPaging);
            if (pageableView != null && pageableView.ShowPager)
            {
                int current = 0;
                if (!Int32.TryParse(PageCurrent.Text, out current))
                    current = 1;

                var pageSize = pageableView.PageSize;
                var pageCount = (int)Math.Ceiling((decimal)pageableView.GetTotalCount() / (decimal)pageSize);
                if (current < 1) current = 1;
                if (current > pageCount) current = pageCount;

                PageCurrent.Text = current.ToString();
                PageTotal.Text = pageCount.ToString();
                Pager.Visible = true;

                int offset = (current - 1) * pageSize;
                data = pageableView.GetData(offset, pageSize);
            }
            else
            {
                data = view.GetData();
            }
            return data;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            OuterPanel.Text = repository.Name + ": " + view.Name;
            viewControl.DataSource = GetData(view);
            viewControl.DataBind();
            base.Render(writer);
        }
    }
}