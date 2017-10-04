using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Eksponent.Dewd.Controls;
using System.Xml.Linq;
using System.Data;
using Eksponent.Dewd.Extensions;

namespace Eksponent.Dewd.Views.Buttons
{
    public class ExportToExcelButton : CustomButton
    {
        const string X_NS_SS = "urn:schemas-microsoft-com:office:spreadsheet";
        static XName X_TABLE_ELEMENT = XName.Get("Table", X_NS_SS);

        public ExportToExcelButton(XElement buttonElement)
            : base(buttonElement)
        {
            Icon = Configuration.BaseUrl + "excel.png";
            AltText = "Export to Microsoft Excel";
        }

        public override void OnClick(object sender, EventArgs e)
        {
            Trace.Info("ExportToExcel½Button: {0}", sender);
            var data = Context.View.GetData();
            var dt = (data is DataTable ? (DataTable)data : ((IEnumerable<object>)data).AsDataTable());
            var xml = @"<?xml version=""1.0""?>" + GetExcelXml(dt).ToString();

            var fileName = String.Format("{0}.xml", Context.Repository.Name);

            var ctx = HttpContext.Current;
            var bytes = ctx.Response.ContentEncoding.GetBytes(xml);
            ctx.Response.ClearHeaders();
            ctx.Response.AppendHeader("Content-Disposition", "attachment; filename=" + fileName);
            //ctx.Response.AppendHeader("Content-Length", xml.Length.ToString());
            ctx.Response.AppendHeader("Content-Length", bytes.Length.ToString(CultureInfo.InvariantCulture));
            ctx.Response.ContentType = "application/vnd.ms-excel";
            //ctx.Response.ContentType = "text/xml";
            ctx.Response.Write(xml);
            ctx.Response.End();
        }

        public static XDocument GetExcelXml(DataTable dt)
        {
            var doc = XDocument.Parse(@"<?xml version=""1.0""?>
                <?mso-application progid=""Excel.Sheet""?>
                <Workbook xmlns=""urn:schemas-microsoft-com:office:spreadsheet""
                 xmlns:o=""urn:schemas-microsoft-com:office:office""
                 xmlns:x=""urn:schemas-microsoft-com:office:excel""
                 xmlns:ss=""urn:schemas-microsoft-com:office:spreadsheet""
                 xmlns:html=""http://www.w3.org/TR/REC-html40"">
                 <DocumentProperties xmlns=""urn:schemas-microsoft-com:office:office"">
                 </DocumentProperties>
                 <Styles>
                  <Style ss:ID=""Default"" ss:Name=""Normal"">
                   <Alignment ss:Vertical=""Bottom""/>
                   <Borders/>
                   <Font ss:FontName=""Calibri"" x:Family=""Swiss"" ss:Size=""11"" ss:Color=""#000000""/>
                   <Interior/>
                   <NumberFormat/>
                   <Protection/>
                  </Style>
                  <Style ss:ID=""sBold"">
                   <Font ss:FontName=""Calibri"" x:Family=""Swiss"" ss:Size=""11"" ss:Color=""#000000"" ss:Bold=""1""/>
                  </Style>
                  <Style ss:ID=""sShortDate"">
                   <NumberFormat ss:Format=""Short Date""/>
                  </Style>
                  <Style ss:ID=""sGeneralDate"">
                   <NumberFormat ss:Format=""General Date""/>
                  </Style>
                 </Styles>
                 <Worksheet ss:Name=""Data"">
                  <Table />
                 </Worksheet>
                </Workbook>
            ");
            var table = doc.Descendants(X_TABLE_ELEMENT).First();

            var headerCells =
                dt.Columns.OfType<DataColumn>().Select(dc =>
                    new CellData()
                    {
                        Data = dc.Caption,
                        DataType = typeof(string),
                        StyleID = "sBold"
                    }).ToList();
            table.Add((new RowData() { Cells = headerCells }).GetRowXml());

            var rows = dt.Rows.OfType<DataRow>()
                .Select(r => new RowData()
                {
                    Cells = r.ItemArray.Select(i => new CellData() { Data = i, DataType = i.GetType() }).ToList()
                });
            table.Add(rows.Select(rd => rd.GetRowXml()));

            return doc;
        }

        class RowData
        {
            static XName X_ROW_ELEMENT = XName.Get("Row", X_NS_SS);
            public List<CellData> Cells { get; set; }

            public XElement GetRowXml()
            {
                return new XElement(X_ROW_ELEMENT,
                    Cells.Select(cd => cd.GetCellXml()));
            }
        }

        class CellData
        {
            public Type DataType { get; set; }
            public object Data { get; set; }
            public string StyleID { get; set; }
            static XName X_STYLE_ATTR = XName.Get("StyleID", X_NS_SS);
            static XName X_TYPE_ATTR = XName.Get("Type", X_NS_SS);
            static XName X_CELL_ELEMENT = XName.Get("Cell", X_NS_SS);
            static XName X_DATA_ELEMENT = XName.Get("Data", X_NS_SS);

            public XElement GetCellXml()
            {
                var dataType = "String";
                if (DataType.IsNumeric())
                    dataType = "Number";

                if (DataType.IsDateTime())
                {
                    StyleID = "sGeneralDate";
                    dataType = "DateTime";
                    Data = String.Format("{0:s}", Data);
                }

                if (DataType.IsBoolean())
                {
                    Data = ((bool)Data) ? "1" : "0";
                    dataType = "Number";
                }

                var cell = new XElement(X_CELL_ELEMENT,
                        new XElement(X_DATA_ELEMENT,
                            new XAttribute(X_TYPE_ATTR, dataType),
                            String.Format("{0}", Data)));
                if (!String.IsNullOrEmpty(StyleID))
                    cell.Add(new XAttribute(X_STYLE_ATTR, StyleID));

                return cell;
            }
        }
    }
}