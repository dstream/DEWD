using System;   
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;
using Eksponent.Dewd.Extensions;
using System.Xml.Linq;
using System.Web.UI;

namespace Eksponent.Dewd.Controls.EditorControls
{
    public class DuoSelectDataType : BaseDataType, IDataType
    {
        #region DropDownDataType
        private IDataEditor _editor;
        private IData _baseData;

        public override IData Data
        {
            get { return _baseData ?? (_baseData = new DefaultData(this)); }
        }

        public override IDataEditor DataEditor
        {
            get { return _editor ?? (_editor = new DuoSelectControl(Data)); }
        }

        public override string DataTypeName
        {
            get { return "Dewd DuoSelect"; }
        }

        public override Guid Id
        {
            get {  return new Guid("ada139a2-d000-4bcc-a351-e019a112da0a"); }
        }

        public override IDataPrevalue PrevalueEditor
        {
            get { return new DefaultPreValueEditor(this, false); }
        }
        #endregion

        public class DuoSelectControl : WebControl, IDataEditor, IXConfigurable
        {
            private IData _data;
            public bool Sortable { get; set; }
            private HiddenField valueField;
            private ListBox listBox;

            public DuoSelectControl(IData data)
            {
                _data = data;
            }

            public System.Web.UI.Control Editor
            {
                get { return this; }
            }

            public void Save()
            {
                _data.Value = valueField.Value;
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
                Page.ClientScript.RegisterClientScriptInclude("DuoSelectDataType", 
                    this.Page.ClientScript.GetWebResourceUrl(this.GetType(), 
                        "Eksponent.Dewd.Controls.EditorControls.DuoSelectDataType.js"));

                Controls.Add(valueField = new HiddenField() { Value = String.Format("{0}", _data.Value) });
                Controls.Add(listBox = new ListBox() { SelectionMode = ListSelectionMode.Multiple, Height = Height });
                listBox.Style.Value = "min-width:180px";

                base.OnInit(e);
            }

            protected override void Render(System.Web.UI.HtmlTextWriter writer)
            {
                writer.Write("<table class='duoSelect'><tr>");
                writer.Write("<td>All:<br />");
                base.Render(writer);
                writer.Write("</select></td>");
                writer.Write("<td class='move'><br /><span>&lt;</span>&nbsp;<span>&gt;</span></td>");
                writer.Write("<td>Selected:<br /><select multiple='multiple' style='min-width:180px;height:"+Height.ToString()+"'></select></td>");
                if(Sortable)
                    writer.Write("<td class='sort' style='font-size:80%'><br /><span>up</span><br /><br /><span>down</span></td>");
                writer.Write("</tr></table>");
            }

            //public void ConfigureControl(XElement fieldElement)
            public XElement ConfigurationElement
            {
                set
                {
                    Init += (obj, e) =>
                    {
                        listBox.Items.AddRange(ListControlHelper.GetItemsFromConfiguration(value));
                    };

                    var selectElement = value.Element("select");
                    if (selectElement != null)
                    {
                        Sortable = selectElement.GetAttribute<bool>("sortable");
                        Height = new Unit(selectElement.GetAttribute("height", "100px"));
                    }
                }
                get
                {
                    throw new NotImplementedException();
                }
            } 
        }
    }
}