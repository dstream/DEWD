using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Eksponent.Dewd.Controls;
using Eksponent.Dewd.Extensions;
using Eksponent.Dewd.Fields;
using Eksponent.Dewd.Fields.Validators;
using Eksponent.Dewd.Repositories;

namespace Eksponent.Dewd.Editors
{
    /// <summary>
    /// Base class for editors supporting validation and events.
    /// </summary>
    public abstract class Editor : IEditor, IRequiresPageAccess, IRequiresMenuButtons
    {
        private List<IField> fields;
        protected Dictionary<string, object> initialFieldValues;
        //protected const string X_SOURCEFIELD = "sourceField";
        protected XElement editorElement;
        protected IRepository repository;
        private System.Web.UI.Page page;

        public event EventHandler<BeforeSaveEventArgs> BeforeSave;
        public event EventHandler<AfterSaveEventArgs> AfterSave;
        public event EventHandler<BeforeDeleteEventArgs> BeforeDelete;
        public event EventHandler<RowEventArgs> AfterDelete;
        public event EventHandler<EventArgs> PagePreInit;
        public event EventHandler<EventArgs> PageInit;
        public event EventHandler<EventArgs> PageLoad;

        public Editor(XElement editorElement, IRepository repository)
        {
            this.editorElement = editorElement;
            this.repository = repository;

            // add validation event
            BeforeSave += BeforeSaveValidate;

            // handle events configured in xml
            HandleConfiguredEvents();
        }

        void HandleConfiguredEvents()
        {
            foreach (XElement eventElement in editorElement.Elements("event"))
            {
                var handler = eventElement.GetAttribute("handler");
                switch (eventElement.GetAttribute("name"))
                {
                    case "BeforeSave": BeforeSave += (sender, e) => { MethodCaller.Invoke(handler, sender, e); }; break;
                    case "AfterSave": AfterSave += (sender, e) => { MethodCaller.Invoke(handler, sender, e); }; break;
                    case "BeforeDelete": BeforeDelete += (sender, e) => { MethodCaller.Invoke(handler, sender, e); }; break;
                    case "AfterDelete": AfterDelete += (sender, e) => { MethodCaller.Invoke(handler, sender, e); }; break;
                    //case "PagePreInit": PagePreInit += (sender, e) => { MethodCaller.Invoke(handler, sender, e); }; break;
                    case "PageInit": PageInit += (sender, e) => { MethodCaller.Invoke(handler, sender, e); }; break;
                    case "PageLoad": PageLoad += (sender, e) => { MethodCaller.Invoke(handler, sender, e); }; break;
                }
            }
        }

        void BeforeSaveValidate(object sender, BeforeSaveEventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append("<ul>");
            foreach (IField field in Fields)
            {
                if (!e.Values.ContainsKey(field.UniqueID))
                    continue; // not editor control field (so ignored in terms of validation)
                var value = e.Values[field.UniqueID];

                // loop over each validation node and do validation
                foreach (XElement valElement in field.Xml.Elements("validation"))
                {
                    // get validator
                    var validator = TypeInstantiater.GetInstance<IValidator>(
                        valElement.GetAttribute("type"),
                        typeof(Required),
                        new string[] { "Eksponent.Dewd.Fields.Validators.{0},Eksponent.Dewd" },
                        valElement);
                    if (!validator.IsValid(value))
                    {
                        // halt further validation on current field
                        e.Cancel = true;
                        var fieldTitle = field.Xml.GetAttribute("title");
                        var tabTitle = field.Xml.GetAttribute("tabTitle");
                        if (!String.IsNullOrEmpty(tabTitle))
                            fieldTitle = tabTitle + " - " + fieldTitle;
                        sb.AppendFormat("<li>" + validator.ErrorText + "</li>", fieldTitle);
                        break; 
                    }
                }

            }
            sb.Append("</ul>");
            if (e.Cancel)
                e.Result = new SaveResult() { Success = false, ErrorMessage = sb.ToString() };
        }

        protected virtual object NullValue
        {
            get { return null; }
        }

        protected virtual object GetParameterValue(IField field, Dictionary<string, object> values)
        {
            var df = (field as ISupportsDefaultValue);

            object val;
            if (values.ContainsKey(field.UniqueID))
                // get value from values returned from editing page
                val = values[field.UniqueID];
            else
                // not an editor control field, so get initial field value; for new row this will
                // be default value and for existing row, it'll be db-value
                val = initialFieldValues[field.UniqueID];

            if (df != null)
            {
                // check if the current value triggers the default value instead
                if (df.TriggersDefault(val))
                    val = df.DefaultValue;
            }

            var nullable = field.Xml.Element("nullable");
            if (nullable != null)
            {
                if (String.Format("{0}", val) == nullable.GetAttribute("whenValueEquals"))
                    val = NullValue;
            }

            // allow value to be manipulated just before db-save
            field.ProcessValueBeforeSave(ref val);
            return val;
        }

        public abstract SaveResult ExecuteSave(ref RowID id, Dictionary<string, object> values);

        public virtual SaveResult Save(ref RowID id, Dictionary<string, object> values)
        {
            Context.RowID = id;

            if (BeforeSave != null)
            {
                var bse = new BeforeSaveEventArgs() { ID = id, Values = values };
                BeforeSave(this, bse);
                if (bse.Cancel)
                    return bse.Result;
            }

            // execute real stuff here
            var result = ExecuteSave(ref id, values);

            if (AfterSave != null)
                AfterSave(this, new AfterSaveEventArgs() { ID = id, Values = values });

            return result ?? new SaveResult() { Success = true };
        }

        public abstract void ExecuteDelete(RowID id);

        public virtual void Delete(RowID id)
        {
            Context.RowID = id;

            if (BeforeDelete != null)
            {
                var bde = new BeforeDeleteEventArgs() { ID = id };
                BeforeDelete(this, bde);
                if (bde.Cancel)
                    return;
            }

            // do actual deletion here
            ExecuteDelete(id);

            if (AfterDelete != null)
                AfterDelete(this, new RowEventArgs() { ID = id });
        }

        protected virtual List<IField> Fields
        {
            get {
                if (fields == null)
                {
                    // initalize fields
                    fields = this.GetFields(
                        editorElement,
                        typeof(Field),
                        new string[] { 
                            "Eksponent.Dewd.Repositories.Table.{0},Eksponent.Dewd", 
                            "Eksponent.Dewd.Repositories.Object.{0},Eksponent.Dewd"
                        });
                }

                return fields; 
            }
        }

        protected virtual IEnumerable<IField> SourceFields
        {
            get { return Fields.Where(f => (f is IHasSourceField) && !String.IsNullOrEmpty((f as IHasSourceField).SourceField)); }
        }

        protected virtual IEnumerable<IField> WriteableFields
        {
            get { return SourceFields.Where(f => !f.Xml.GetAttribute<bool>("readonly")); }
        }

        protected abstract Dictionary<string, object> GetRowValues(RowID id);

        public virtual IEnumerable<ContentControlDefinition> GetContentControlDefinitions(RowID id)
        {
            var controlDefs = Fields.Select(fx => fx.ContentControlDefinition).Where(cd => cd != null).ToList();

            // initialize default values
            initialFieldValues = new Dictionary<string, object>();
            Fields.ForEach(f =>
            {
                var df = (f as ISupportsDefaultValue);
                if (df != null)
                {
                    var defaultVal = df.DefaultValue;
                    if (defaultVal != null)
                        initialFieldValues[f.UniqueID] = defaultVal;
                }
            });
            
            // get values from database
            if (id != null)
            {
                var rowValues = GetRowValues(id);
                Fields.ForEach(f =>
                {
                    // if field retrieves value internally
                    var val = f.RetrieveValue(id);
                    if (val != null)
                        rowValues[f.UniqueID] = val;
                    
                    // process the value before its sent to the control for editing
                    if (rowValues.ContainsKey(f.UniqueID))
                    {
                        object editValue = rowValues[f.UniqueID];
                        f.ProcessValueBeforeEdit(ref editValue);
                        initialFieldValues[f.UniqueID] = editValue;
                    }
                });
            }

            // set initial editor control values
            controlDefs.ForEach(def => { if(initialFieldValues.ContainsKey(def.ID)) def.InitialValue = initialFieldValues[def.ID]; });

            return controlDefs;
        }

        public System.Web.UI.Page PageContext
        {
            get {
                if (page == null)
                    throw new FormattedException("Page is not available till after PagePreInitCallBack.");
                return page; 
            }
        }

        public T FindControl<T>(string controlId) where T : System.Web.UI.Control
        {
            return (T)page.Master.FindControl("body").FindControl(controlId);
        }

        public ContentControlContainer ContentControlContainer
        {
            get { return (page as IEditorPage).ContentControlContainer; }
        }

        public List<MenuButton> MenuButtons
        {
            get { return ContentControlContainer.MenuButtons; }
        }

        public virtual void PagePreInitCallBack(System.Web.UI.Page page)
        {
            this.page = page;
            page.Init += (obj, args) => {
                // handle html injection nodes i config
                UiHelper.InjectHtml(page, editorElement);

                if (PageInit != null) PageInit(obj, args); 
            };
            page.Load += (obj, args) => { if (PageLoad != null) PageLoad(obj, args); };
        }

        public virtual void AddMenuButtons()
        {
            MenuButtons.AddRange(
                CustomButton.GetCustomButtons(editorElement).Select(b => b.GetMenuButton()));
        }
    }
}