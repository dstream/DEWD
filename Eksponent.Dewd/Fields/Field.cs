using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using Eksponent.Dewd.Extensions;
using Eksponent.Dewd.Editors;
using System.Globalization;
using Eksponent.Dewd.Fields.Criteria;
using Eksponent.Dewd.Controls;
using umbraco.cms.businesslogic.datatype;


namespace Eksponent.Dewd.Fields
{
    /// <summary>
    /// Data source neutral implementation of IField with default value support.
    /// </summary>
    public class Field : IField, ISupportsDefaultValue, IHasSourceField
    {
        public XElement Xml { get; private set; }
        private IEditor editor;
        private const string RQT_KEY_CULTURE = "Field.PageCulture";
        protected const string X_UMBRACOTYPE = "umbracoDataType";
        protected const string X_SOURCEFIELD = "sourceField";

        public string UniqueID
        {
            get
            {
                return Xml.GetAttribute("tabTitle") + "_" + Xml.GetAttribute("title");
            }
        }

        public Field(XElement fieldElement, IEditor editor)
        {
            Xml = fieldElement;
            this.editor = editor;
        }

        public override int GetHashCode()
        {
            return UniqueID.GetHashCode();
        }

        public bool Equals(IField field)
        {
            return UniqueID == field.UniqueID;
        }

        public override bool Equals(object obj)
        {
            var sf = obj as IField;
            if (sf == null)
                return false;

            return Equals(sf);
        }

        public virtual object RetrieveValue(RowID id)
        {
            return null;
        }

        public virtual CultureInfo Culture
        {
            get
            {
                // check for explicit culture
                string cultureStr = Xml.GetAttribute("culture");
                if (cultureStr.Length != 0)
                    return CultureInfo.GetCultureInfo(cultureStr);

                // note: for some reason the thread culture change during page life cycle, so we cache the initial value
                var cachedCulture = RequestTemp.Get<CultureInfo>(RQT_KEY_CULTURE);
                if (cachedCulture != null)
                    return cachedCulture;

                var culture = CultureInfo.CurrentCulture;
                RequestTemp.Put(RQT_KEY_CULTURE, culture);
                return culture;
            }
        }

        public virtual void ProcessValueBeforeEdit(ref object value)
        {
            // allow custom string formatting to be applied to the value
            string format = Xml.GetAttribute("format");
            if (format.Length != 0)
                value = String.Format(Culture, format, value);
        }

        public virtual void ProcessValueBeforeSave(ref object value)
        {
            // allow a type conversion to occur
            var targetType = Xml.GetAttribute("targetType");
            if (targetType.Length != 0)
            {
                var type = TypeInstantiater.SearchType(targetType, typeof(string), new string[] { "System.{0}" });
                value = Convert.ChangeType(value, type);
            }
        }

        protected virtual IEditor Editor
        {
            get
            {
                return this.editor;
            }
        }

        public virtual object DefaultValue
        {
            get
            {
                // check for advanced defaultValue
                var advDefault = Xml.Element("defaultValue");
                if (advDefault != null)
                    return ValueGetters.ValueGetter.GetFromValueGetter(advDefault);

                // support for simple default value
                var initialValue = Xml.GetAttribute("defaultValue");
                if (initialValue.Length != 0)
                    return initialValue;

                return null;
            }
        }

        public bool TriggersDefault(object value)
        {
            return Criteria.Criterion.IsAnyTrue(
                Xml.XPathSelectElements("defaultValue/revertTrigger"), value, typeof(IsNullOrEmpty));
        }

        private DataTypeDefinition GetUmbracoDataTypeDefinition(string id)
        {
            int dtdId = 0;
            if (Int32.TryParse(id, out dtdId))
                return DataTypeDefinition.GetDataTypeDefinition(dtdId);

            var def = DataTypeDefinition.GetAll().FirstOrDefault(d => d.Text.Equals(id, StringComparison.InvariantCultureIgnoreCase));
            if(def==null)
                throw new FormattedException("Umbraco datatype definition not found '{0}'", id);

            return def;
        }

        public ContentControlDefinition ContentControlDefinition
        {
            get
            {
                if (Xml.GetAttribute(X_UMBRACOTYPE).Length == 0)
                    return null;

                return new ContentControlDefinition()
                {
                    ID = UniqueID,
                    Title = Xml.GetAttribute("title"),
                    Caption = (Xml.Element("caption") != null ? Xml.Element("caption").Value : ""),
                    TabTitle = Xml.GetAttribute("tabTitle", "Data"),
                    DataTypeDefinition = GetUmbracoDataTypeDefinition(Xml.GetAttribute(X_UMBRACOTYPE)),
                    InitializeTypeInstanceCallBack = (dt =>
                    {
                        var xc = (dt.DataEditor.Editor as IXConfigurable);
                        if (xc != null)
                            xc.ConfigurationElement = Xml;
                    })
                };
            }
        }

        public string SourceField
        { 
            get { return Xml.GetAttribute(X_SOURCEFIELD); }
        }
    }
}