using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Eksponent.Dewd.Views;
using Eksponent.Dewd.Extensions;
using Eksponent.Dewd.Editors;

namespace Eksponent.Dewd.Repositories
{
    public abstract class Repository : IRepository
    {
        private string primaryKey = "ID";
        private XElement element;
        private List<IView> views;
        private IEditor editor;

        protected abstract Type DefaultViewType { get; }
        protected abstract Type DefaultEditorType { get; }

        public Repository(XElement element)
        {
            this.element = element;
        }

        public List<IView> GetViews(XElement repositoryElement, IRepository repository, Type defaultView)
        {
            var views = new List<IView>();
            foreach (XElement viewElement in repositoryElement.Elements("view"))
            {
                var view = TypeInstantiater.GetInstance<IView>(
                    viewElement.GetAttribute("type"),
                    defaultView,
                    new string[] { "Eksponent.Dewd.Views.{0},Eksponent.Dewd" },
                    viewElement,
                    repository);
                views.Add(view);
            }
            return views;
        }

        public virtual List<IView> Views
        {
            get
            {
                if (views == null)
                    views = GetViews(element, this, DefaultViewType);
                return views;
            }
        }

        public IEditor GetEditor(XElement repositoryElement, IRepository repository, Type defaultEditor)
        {
            var editorElement = repositoryElement.Element("editor");
            if (editorElement == null)
            {
                Trace.Warn("editorElement not found {0}", repositoryElement.ToString());
                return null;
            }

            return TypeInstantiater.GetInstance<IEditor>(
                editorElement.GetAttribute("type"),
                defaultEditor,
                editorElement,
                repository);
        }

        public virtual IEditor Editor
        {
            get
            {
                if (editor == null)
                    editor = GetEditor(element, this, DefaultEditorType);
                return editor;
            }
        }

        public virtual string PrimaryKeyName
        {
            get
            {
                var pk = element.Element("primaryKey");
                if (pk != null)
                    primaryKey = pk.GetAttribute("name", primaryKey);
                return primaryKey;
            }
        }

        public virtual bool PrimaryKeyValueIsManual
        {
            get
            {
                var pk = element.Element("primaryKey");
                if (pk != null)
                    return pk.GetAttribute<bool>("manual");
                return false;
            }
        }

        public string Name
        {
            get { return element.GetAttribute("name"); }
        }
    }
}