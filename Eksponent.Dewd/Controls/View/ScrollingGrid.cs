using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Eksponent.Dewd.Repositories;
using Eksponent.Dewd.Views;
using Eksponent.Dewd.Extensions;
using System.Data;
using System.Web.UI;
using System.Xml.Linq;
using Eksponent.Dewd.Controls.View.Columns;
using System.Collections.Specialized;

namespace Eksponent.Dewd.Controls.View
{
    public class ScrollingGrid : GridView, IViewControl, ISupportsMultiSelection
    {
        public virtual bool EnableEdit { get; set; }
        public virtual IRepository Repository { get; set; }
        public virtual IView View { get; set; }
        public bool EnableMultiSelection { get; set; }
        private MultiSelectBox multiSelectColumn;
        //private List<DataControlField> columns;

        public XElement ViewElement
        {
            get
            {
                if (View is IXConfigurable)
                    return ((IXConfigurable)View).ConfigurationElement;
                return null;
            }
        } 

        protected override void OnInit(EventArgs e)
        {
            CssClass="grid"; 
            GridLines = System.Web.UI.WebControls.GridLines.None;
            Style.Value = "width:100%";
            UseAccessibleHeader = true;

            base.OnInit(e);
        }

        protected override void DataBind(bool raiseOnDataBinding)
        {
            base.DataBind(raiseOnDataBinding);
            HeaderRow.TableSection = TableRowSection.TableHeader;
        }

        protected string GetKey(object obj)
        {
            if (obj is DataRow)
                return String.Format("{0}", ((DataRow)obj)[Repository.PrimaryKeyName]);
            return DataBinder.Eval(obj, Repository.PrimaryKeyName, "{0}");
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (EnableEdit)
            {                
                string editUrl = String.Format(
                    Configuration.BaseUrl + "Edit.aspx?txe={0}&amp;id=", Page.Server.UrlPathEncode(Page.Request["txe"]));
                Columns.Insert(0, new HtmlField()
                {
                    GenerateHtmlCallBack = (dataItem) => { return String.Format("<a href='{0}'>Edit</a>", editUrl + GetKey(dataItem)); }
                });
            }
            base.OnPreRender(e);
        }

        protected override System.Collections.ICollection CreateColumns(PagedDataSource dataSource, bool useDataSource)
        {
            var cols = new List<DataControlField>(base.CreateColumns(dataSource, useDataSource).Cast<DataControlField>());

            // display only certain columns?
            var columnElement = ViewElement.Element("columns");
            if (columnElement != null)
            {
                if (columnElement.HasElements)
                {
                    var customCols = new List<DataControlField>();
                    if (EnableEdit)
                        customCols.Add(cols.OfType <HtmlField>().First());
                    foreach (XElement xc in columnElement.Elements("column"))
                    {
                        var col = TypeInstantiater.GetInstance<DataControlField>(
                            xc.GetAttribute("type"),
                            typeof(HtmlField),
                            new string[] {
                                "Eksponent.Dewd.Controls.View.Columns.{0},Eksponent.Dewd"
                            }, 
                            xc);
                        var reqCtx = (col as IRequiresContext);
                        if (reqCtx != null)
                        {
                            reqCtx.RepositoryContext = Repository;
                            reqCtx.ViewContext = View;
                        }

                        customCols.Add(col);
                    }
                    cols = customCols;
                }
                else
                {
                    // simple comma-separated list of column names
                    var visible = columnElement.Value.Split(',').Select(s => s.Trim().ToLower()).ToList();
                    cols = cols.Cast<DataControlField>()
                        .Where(d =>
                        {
                            if (d is BoundField)
                                return visible.Contains(((BoundField)d).DataField.ToLower());
                            return true;
                        }).OrderBy(d => {
                            if (d is BoundField)
                                return visible.IndexOf(((BoundField)d).DataField.ToLower());
                            return 0;
                        }).ToList();
                }
            }

            if (EnableMultiSelection)
                cols.Add(multiSelectColumn = new MultiSelectBox() { RepositoryContext = Repository, ViewContext = View });

            // call ProcessColumn 
            cols.Cast<DataControlField>().ToList().ForEach(ProcessColumn);
            //columns = cols;

            return cols;
        }

        public virtual void ProcessColumn(DataControlField field)
        {
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            writer.Write(@"
                <style type=""text/css"">        
                    .scrollingGrid table td, .scrollingGrid table th { padding: 3px; border:1px solid #e0e0e0; }
                    .scrollingGrid table th { padding: 3px; background:url(../../images/topGradient.gif) repeat-x 50% top; color:#000000 }
                    .scrollingGrid .grid { border-left: 1px solid #e0e0e0; }
                    .scrollingGrid .grid tr.hover { background: #f0f0f0; }
                </style>

                <script type=""text/javascript"">
                    jQuery(function () { dewd.scrollgrid(jQuery('.scrollingGrid'), jQuery('.content'), 17 + 35 + 5 + 30 + 30); });
                </script>

                <div class=""scrollingGrid"">
                    <div style=""height:200px;overflow:auto;overflow-x:hidden;border-bottom: 1px solid #e0e0e0;"" class=""scroll"">
            ");
            if (DataSource == null || this.Rows.Count==0)
                writer.Write("No rows to display in the current view.");
            else
                base.Render(writer);
            writer.Write("</div></div>");
        }

        public RowID[] GetSelectedRowIDs()
        {
            // todo: need to change View.aspx binding logic to get the optimal way to extract ids to work
            /*
            //var colIndex = columns.Cast<DataControlField>().ToList().FindIndex(d => d is MultiSelectBox);
            //Trace.Info("Index {0} of {1}", colIndex, columns.Count);
            foreach (GridViewRow row in Rows)
            {
                var dic = new OrderedDictionary();
                multiSelectColumn.ExtractValuesFromCell(
                    dic, row.Cells[3] as DataControlFieldCell, DataControlRowState.Normal, false);
                Trace.Info("{0}", dic.Count);
            }*/

            // instead we do a hack
            var list = new List<RowID>();
            foreach (string key in HttpContext.Current.Request.Form.AllKeys)
            {
                if (key.EndsWith("MultiSelectBox"))
                    list.Add(RowID.Get(HttpContext.Current.Request.Form[key]));
            }
            return list.ToArray();
        }
    }
}