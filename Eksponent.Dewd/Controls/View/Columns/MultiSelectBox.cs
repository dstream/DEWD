using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI;
using Eksponent.Dewd.Views;
using Eksponent.Dewd.Repositories;
using System.Web.UI.HtmlControls;

namespace Eksponent.Dewd.Controls.View.Columns
{
    public class MultiSelectBox : DataControlField, IRequiresContext
    {
        public IView ViewContext { get; set; }
        public IRepository RepositoryContext { get; set; }

        public override bool Initialize(bool sortingEnabled, Control control)
        {
            /*
            var view = ViewContext as Eksponent.Dewd.Views.View;
            if (view != null)
            {
                view.BottomRightPanel.Controls.Add(new HyperLink() { Text = "Select/Deselect" });
            }*/

            return base.Initialize(sortingEnabled, control);
        }

        public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
        {
            base.InitializeCell(cell, cellType, rowState, rowIndex);

            if (cellType == DataControlCellType.DataCell)
            {
                cell.DataBinding += (sender, args) =>
                {
                    object dataItem = DataBinder.GetDataItem(cell.NamingContainer);
                    string rowId = DataBinder.Eval(dataItem, RepositoryContext.PrimaryKeyName, "{0}");
                    cell.HorizontalAlign = HorizontalAlign.Center;
                    cell.Controls.Add(new HtmlInputCheckBox()
                    {
                        ID = "MultiSelectBox",
                        Value = rowId
                    });
                };
            }
        }

        public override void ExtractValuesFromCell(System.Collections.Specialized.IOrderedDictionary dictionary, DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly)
        {
            base.ExtractValuesFromCell(dictionary, cell, rowState, includeReadOnly);

            var checkBox = (HtmlInputCheckBox)cell.Controls[0];
            dictionary.Add(cell, checkBox.Checked ? checkBox.Value : "");
        }

        protected override DataControlField CreateField()
        {
            return new MultiSelectBox();
        }
    }
}