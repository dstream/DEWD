using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Xml.Linq;
using Eksponent.Dewd.Extensions;

namespace Eksponent.Dewd.Controls.View.Columns
{
    public class HtmlField : System.Web.UI.WebControls.DataControlField
    {
        public HtmlFieldGenerateDelegate GenerateHtmlCallBack { get; set; }
        private XElement xc;
        [ThreadStatic]
        private static int i = 0;
        protected string[] fields;

        public HtmlField()
        {
        }

        public HtmlField(XElement columnElement)
        {
            xc = columnElement;

            // get fields
            fields = xc.GetAttribute("field")
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).ToArray();

            // get format, title, width
            string format = xc.GetAttribute("format", "{0}");
            HeaderText = xc.GetAttribute("title", xc.GetAttribute("field", String.Format("Field {0}", i++)));
            string width = xc.GetAttribute("width");
            if (width.Length != 0)
                ItemStyle.Width = new Unit(width);
            ItemStyle.Wrap = !xc.GetAttribute<bool>("nowrap");

            // LE for formatting it
            GenerateHtmlCallBack = (obj) =>
            {
                var values = fields.Select(f => DataBinder.Eval(obj, f)).ToArray();
                return String.Format(format, values);
            };
        }

        public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
        {
            base.InitializeCell(cell, cellType, rowState, rowIndex);

            if (cellType == DataControlCellType.DataCell && GenerateHtmlCallBack!=null)
            {
                cell.DataBinding += (sender, args) => {
                    object dataItem = DataBinder.GetDataItem(cell.NamingContainer);
                    cell.Controls.Add(new LiteralControl(GenerateHtmlCallBack(dataItem)));
                };
            }
        }

        protected override DataControlField CreateField()
        {
            return new HtmlField();
        }
    }

    public delegate string HtmlFieldGenerateDelegate(object dataItem);
}