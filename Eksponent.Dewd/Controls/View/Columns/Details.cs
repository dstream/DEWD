using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Web.UI.WebControls;
using Eksponent.Dewd.Extensions;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Eksponent.Dewd.Controls.View.Columns
{
    public class Details : DataControlField
    {
        private XElement _columnElement;
        public string[] SourceFields { get; set; }
        public string HtmlTemplate { get; set; }
        private const string MODAL_SCRIPT = 
            "jQuery(function () { jQuery('.gdmLink').click(function () { jQuery(this).next().jqmShow(); }).next().addClass('gridModal').jqm({ toTop : true, modal : false }); });";
        public string LinkText { get; set; } 

        public Details()
        {
            LinkText = "Details";
        }

        public Details(XElement xc) : this()
        {
            _columnElement = xc;
            HeaderText = xc.GetAttribute("title");
            SourceFields = xc.GetAttribute("field")
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).ToArray();
            LinkText = xc.GetAttribute("linkText", LinkText);
        }

        public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
        {
            base.InitializeCell(cell, cellType, rowState, rowIndex);

            if (cellType == DataControlCellType.DataCell)
            {
                this.Control.Page.ClientScript.RegisterClientScriptBlock(typeof(Details), "Modal", MODAL_SCRIPT, true);

                cell.DataBinding += (sender, args) =>
                {
                    object dataItem = DataBinder.GetDataItem(cell.NamingContainer);

                    if (String.IsNullOrEmpty(HtmlTemplate) && String.IsNullOrEmpty(HtmlTemplate = _columnElement.Value))
                    {
                        // auto build html template from source fields
                        if (SourceFields.Length == 0)
                        {
                            throw new NotSupportedException("Fields must be specified.");
                        }                        

                        int fi = 0;
                        HtmlTemplate = "<h2 style='margin-top:0px'>Details</h2>" + 
                            String.Join("<br />", SourceFields.Select(f => String.Format("<h3 style='margin-bottom:0px'>{0}:</h3>{{{1}}}", f, fi++)).ToArray());
                        //Trace.Info("HTML: {0}", HtmlTemplate);
                    }

                    cell.Controls.Add(new LiteralControl(String.Format("<a href='javascript:void(0)' class='gdmLink'>{0}</a>", LinkText)));

                    // generate hidden div
                    var values = SourceFields.Select(f => DataBinder.Eval(dataItem, f)).ToArray();
                    var div = new HtmlGenericControl("div") { InnerHtml = String.Format(HtmlTemplate, values) };
                    cell.Controls.Add(div);
                };
            }
        }

        protected override DataControlField CreateField()
        {
            return new Details();
        }
    }
}