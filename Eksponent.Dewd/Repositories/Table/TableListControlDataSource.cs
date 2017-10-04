using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Eksponent.Dewd.Repositories;
using umbraco.DataLayer;
using Eksponent.Dewd.Extensions;
using Eksponent.Dewd.Controls;
using Eksponent.Dewd.Fields.ValueGetters;

namespace Eksponent.Dewd.Repositories.Table
{
    public class TableListControlDataSource : IListControlDataSource
    {
        protected virtual IParameter[] GetSqlParameters(XElement listControlDataSourceElement, ISqlHelper sqlHelper)
        {
            return listControlDataSourceElement.Elements("parameter")
                .Select(x => sqlHelper.CreateParameter(x.GetAttribute("name"), ValueGetter.GetFromValueGetter(x)))
                .ToArray();
        }

        public ListItem[] GetListItems(XElement listControlDataSourceElement)
        {
            var items = new List<ListItem>();
            var dsn = TableRepository.GetNearestConnectionString(listControlDataSourceElement);
            var sql = listControlDataSourceElement.Element("sql").Value;
            using (ISqlHelper sqlHelper = DataLayerHelper.CreateSqlHelper(dsn))
            {
                var dbParams = GetSqlParameters(listControlDataSourceElement, sqlHelper);
                IRecordsReader reader = sqlHelper.ExecuteReader(sql, dbParams.ToArray());
                foreach(System.Data.IDataRecord rc in reader) {
                    items.Add(new ListItem(
                        String.Format("{0}", rc.GetValue(1)),
                        String.Format("{0}", rc.GetValue(0))));
                }                    
            }
            return items.ToArray();
        }
    }
}