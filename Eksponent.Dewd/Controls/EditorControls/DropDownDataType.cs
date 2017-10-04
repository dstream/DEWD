using System;   
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;
using Eksponent.Dewd.Extensions;
using System.Xml.Linq;

namespace Eksponent.Dewd.Controls.EditorControls
{
    public class DropDownDataType : BaseDataType, IDataType
    {
        #region DropDownDataType
        private IDataEditor _editor;
        private IData _baseData;

        public override IData Data
        {
            get { return _baseData ?? (_baseData = new NoSqlDefaultData(this)); }
        }

        public override IDataEditor DataEditor
        {
            get { return _editor ?? (_editor = new DropDownControl(Data)); }
        }

        public override string DataTypeName
        {
            get { return "Dewd DropDown"; }
        }

        public override Guid Id
        {
            get {  return new Guid("2f480d1c-ea9c-4663-89b2-3eafcf07b137"); }
        }

        public override IDataPrevalue PrevalueEditor
        {
            get { return new DefaultPreValueEditor(this, false); }
        }
        #endregion

        public class DropDownControl : DropDownList, IDataEditor, IXConfigurable
        {
            private IData _data;

            public DropDownControl(IData data)
            {
                _data = data;
            }

            public System.Web.UI.Control Editor
            {
                get { return this; }
            }

            public void Save()
            {
                _data.Value = (this.SelectedIndex >= 0 ? this.SelectedValue : "");
            }

            public bool ShowLabel
            {
                get { return true; }
            }

            public bool TreatAsRichTextEditor
            {
                get { return false; }
            }

            protected override void OnInit(EventArgs e)
            {
                if (_data != null && _data.Value != null)
                {
                    this.SelectedValue = _data.Value.ToString();
                }

                base.OnInit(e);
            }

            //public void ConfigureControl(XElement fieldElement)
            public XElement ConfigurationElement
            {
                set
                {
                    this.Items.AddRange(ListControlHelper.GetItemsFromConfiguration(value));
                }
                get {
                    throw new NotImplementedException();
                }
            }
        }
    }
}