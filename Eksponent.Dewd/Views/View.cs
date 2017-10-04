using System;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using Eksponent.Dewd.Repositories;
using Eksponent.Dewd.Extensions;
using Eksponent.Dewd.Controls.View;
using System.Web.UI;
using Eksponent.Dewd.Controls;
using System.Collections.Generic;
using umbraco.uicontrols;
using umbraco;

namespace Eksponent.Dewd.Views
{      
    /// <summary>
    /// View base class supporting paging, searches, control and event access on view page.
    /// </summary>
    public abstract class View : IView, ISupportsPaging, IRequiresPageAccess, IXConfigurable
    {
        private XElement viewElement;
        private IRepository repository;
        private IViewControl viewControl;
        public XElement ConfigurationElement { get; set; }
        private Page page;

        public Control TopLeftPanel { get; private set; }
        public Control TopRightPanel { get; private set; }
        public Control BottomLeftPanel { get; private set; }
        public Control BottomRightPanel { get; private set; }
        public ScrollingMenu Menu { get; private set; }

        public event EventHandler<EventArgs> PageInit;
        public event EventHandler<EventArgs> PageLoad;

        public View(XElement viewElement, IRepository repository)
        {
            this.viewElement = viewElement;
            this.repository = repository;
            ConfigurationElement = viewElement;
        }

        public abstract object GetData(int offset, int count);
        
        public abstract int GetTotalCount();

        public virtual object GetData()
        {
            return GetData(0, 0);
        }

        public XElement ViewElement
        {
            get { return viewElement; }
        }

        public virtual string Name
        {
            get { return viewElement.GetAttribute("name"); }
        }

        public virtual int PageSize
        {
            get { return viewElement.GetAttribute<int>("pageSize", 20); }
        }

        public virtual bool ShowPager
        {
            get { return viewElement.GetAttribute<bool>("showPager", true); }
        }

        public virtual bool AddMenuButtons()
        {
            var addedButtons = false;
            var editor = repository.Editor;
            if (editor != null)
            {
                var createButton = Menu.NewImageButton();
                createButton.ImageURL = Configuration.BaseUrl + "page_add.png";
                createButton.AltText = "Create new ...";
                createButton.Click += (obj, args) => {
                    page.Response.Redirect(
                        Configuration.BaseUrl + "Edit.aspx?txe=" + 
                        page.Server.UrlPathEncode(page.Request["txe"]));
                };

                var multiSelect = (ViewControl as ISupportsMultiSelection);
                if (multiSelect != null)
                {
                    multiSelect.EnableMultiSelection = true;
                    var deleteButton = Menu.NewImageButton();
                    deleteButton.ImageURL = Configuration.BaseUrl + "cross.png";
                    deleteButton.AltText = "Delete selected row(s).";
                    deleteButton.OnClientClick = "return confirm('This will delete all selected rows. Are you sure?')";
                    deleteButton.Click += (obj, args) =>
                    {
                        foreach(RowID rowId in multiSelect.GetSelectedRowIDs())
                            editor.Delete(rowId);
                    };
                }
                addedButtons = true;
            }

            var customList = CustomButton.GetCustomButtons(viewElement);
            if (customList.Count != 0)
            {
                MenuButton.AddButtons(Menu, customList.Select(b => b.GetMenuButton()).ToList());
                addedButtons = true;
            }

            return addedButtons;
        }

        public virtual void PagePreInitCallBack(Page page)
        {
            this.page = page;
            TopLeftPanel = FindControl<Control>("TopLeftPanel");
            TopRightPanel = FindControl<Control>("TopRightPanel");
            BottomLeftPanel = FindControl<Control>("BottomLeftPanel");
            BottomRightPanel = FindControl<Control>("BottomRightPanel");

            var panel = FindControl<UmbracoPanel>("OuterPanel");
            
            page.Init += (obj, args) => {
                Menu = panel.Menu;
                panel.hasMenu = AddMenuButtons();

                // handle html injection nodes i config
                UiHelper.InjectHtml(page, ConfigurationElement);

                if (PageInit != null) PageInit(obj, args); 
            };
            page.Load += (obj, args) => { if (PageLoad != null) PageLoad(obj, args); };
        }

        public Page PageContext
        {
            get { return page; }
        }

        public T FindControl<T>(string controlId) where T : Control
        {
            return (T)page.Master.FindControl("body").FindControl(controlId);
        }

        public IViewControl ViewControl
        {
            get
            {
                if (viewControl == null)
                    throw new ApplicationException("ViewControl is not available until after CreateViewControl or OnInit.");
                return viewControl;
            }
        }

        public virtual IViewControl CreateViewControl()
        {
            viewControl = TypeInstantiater.GetInstance<IViewControl>(
                ConfigurationElement.GetAttribute("controlType"),
                typeof(ScrollingGrid),
                new string[] { "Eksponent.Dewd.Controls.View.{0},Eksponent.Dewd" });
            viewControl.Repository = repository;
            viewControl.View = this;
            return viewControl;
        }
    }
}