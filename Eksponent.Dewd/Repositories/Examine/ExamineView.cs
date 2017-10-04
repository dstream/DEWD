using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Eksponent.Dewd.Views;
using Eksponent.Dewd.Extensions;
using Eksponent.Dewd.Fields.ValueGetters;
using System.Data;
using ExamineCore = global::Examine;

namespace Eksponent.Dewd.Repositories.Examine
{
    public class ExamineView : DataTableBasedView
    {
        public string SearchProviderName { get; set; }
        public string DefaultQuery { get; set; }

        public ExamineView(XElement viewElement, IRepository repository)
            : base(viewElement, repository)
        {
            var allQuery = String.Join(" ", 
                Util.GetSequence(0, 9).Select(i => String.Format("{0}:{1}*", repository.PrimaryKeyName, i)).ToArray());
            DefaultQuery = viewElement.GetAttribute("query", allQuery);
        }

        protected override bool ApplyFiltering { get { return false; } }

        public override bool AddMenuButtons()
        {
            var createButton = Menu.NewImageButton();
            createButton.ImageURL = umbraco.GlobalSettings.Path + "/images/editor/instable.GIF";
            createButton.AltText = "Rebuild index";
            createButton.OnClientClick = "return confirm('Index will be unavailable while rebuilding. Are you sure?')";
            createButton.Click += (obj, args) =>
            {
                ExamineCore.ExamineManager.Instance.RebuildIndex();
                PageContext.Response.Redirect(PageContext.Request.RawUrl);
            };
            return true;
        }

        protected override System.Data.DataTable GetDataTable()
        {
            var manager = ExamineCore.ExamineManager.Instance;

            var searchProvider = SearchProviderName==null ? 
                manager.DefaultSearchProvider : manager.SearchProviderCollection[SearchProviderName];

            var criteria = searchProvider.CreateSearchCriteria();
            var query = (searchPanel == null || !searchPanel.IsSearching ? DefaultQuery : searchPanel.Text);    
            criteria.RawQuery(query);

            var results = searchProvider.Search(criteria);
            Trace.Info("ExamineView got {0} results from {1}.", results.TotalItemCount, query);

            DataTable dt = null;
            foreach (ExamineCore.SearchResult result in results)
            {
                if(dt==null) {
                    dt = new DataTable();
                    foreach (string fieldName in result.Fields.Keys)
                        dt.Columns.Add(fieldName, typeof(string));
                }
                dt.Rows.Add(dt.Columns.Cast<DataColumn>().Select(c => {
                    string value;
                    if (result.Fields.TryGetValue(c.ColumnName, out value))
                        return value;
                    return "";
                }).ToArray());
            }
            return dt;
        }
   }
}