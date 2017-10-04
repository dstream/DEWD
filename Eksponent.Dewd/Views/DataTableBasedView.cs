using System;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using Eksponent.Dewd.Repositories;
using Eksponent.Dewd.Extensions;
using Eksponent.Dewd.Controls.View;
using umbraco.DataLayer;

namespace Eksponent.Dewd.Views
{
    /// <summary>
    /// Simple base class for views that are able to represent the data source as a data table. Does not 
    /// scale to large data sets well, but it should be okay for a couple of thousand rows.
    /// </summary>
    public abstract class DataTableBasedView : View
    {
        private const string RC_KEY = "Dewd.DataTableBasedView.DataTable";
        protected SearchPanel searchPanel;

        public DataTableBasedView(XElement viewElement, IRepository repository) : base(viewElement, repository)
        {
            PageInit += (obj, args) =>
            {
                if(viewElement.GetAttribute<bool>("searchEnabled", true))
                    TopRightPanel.Controls.Add(searchPanel = GetSearchPanel());
            };
        }

        /// <summary>
        /// Returns the default SearchPanel consisting of one textfield.
        /// </summary>
        /// <returns></returns>
        public virtual SearchPanel GetSearchPanel()
        {
            return new SearchPanel();
        }

        public override object GetData()
        {
            return GetData(0, 0);
        }

        protected abstract DataTable GetDataTable();

        /// <summary>
        /// Gets a value indicating whether initial data set should be filtered using the
        /// GetFilteredResult method. Default search/filtering logic can be disabled by overriding
        /// and returning false instead of true.
        /// </summary>
        /// <value><c>true</c> if filtering should be applied; otherwise, <c>false</c>.</value>
        protected virtual bool ApplyFiltering { get { return true; } }

        public override object GetData(int offset, int count)
        {
            var dt = (DataTable)RequestTemp.Get("TableView.Data");
            if (dt == null)
            {
                dt = GetDataTable();
                if(ApplyFiltering)
                    dt = GetFilteredResult(dt);

                if (dt == null)
                    return null;
                RequestTemp.Put("TableView.Data", dt);
            }

            if (offset == 0 && count == 0)
                return dt;
            if (dt.Rows.Count == 0)
                return dt; // no need to page, if no rows

            return dt.Rows.Cast<DataRow>().Skip(offset).Take(count).CopyToDataTable();
        }

        public override int GetTotalCount()
        {
            var dt = (GetData() as DataTable);
            if (dt == null)
                return 0;
            return dt.Rows.Count;
        }

        /// <summary>
        /// Gets the filtered result using the DataTable.RowFilter to do a LIKE operation on all
        /// columns in the table using an OR operator.
        /// </summary>
        /// <param name="originalData">The original data.</param>
        /// <returns></returns>
        public virtual DataTable GetFilteredResult(DataTable originalData)
        {
            if (searchPanel == null || !searchPanel.IsSearching)
                return originalData;

            var query = searchPanel.Text.Replace("'", "''");
            
            var likes = originalData.Columns
                .Cast<DataColumn>()
                .Select(c => String.Format("Convert([{0}], System.String) LIKE '%{1}%'", c.ColumnName, query))
                .ToArray();
            var filter = String.Join(" OR ", likes);
            Trace.Info("Searching: {0}", filter);

            var dv = new DataView(originalData) { RowFilter = filter };
            return dv.ToTable();
        }
    }
}