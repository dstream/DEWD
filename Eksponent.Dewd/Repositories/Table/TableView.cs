using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Eksponent.Dewd.Repositories;
using Eksponent.Dewd.Extensions;
using System.Data;
using umbraco.DataLayer;
using umbraco;
using Eksponent.Dewd.Controls.View;
using Eksponent.Dewd.Views;
using Eksponent.Dewd.Fields.ValueGetters;

namespace Eksponent.Dewd.Repositories.Table
{
    public class TableView : DataTableBasedView
    {
        public TableView(XElement viewElement, IRepository repository) : base(viewElement, repository)
        {
        }

        protected virtual string SQL
        {
            get { return ViewElement.Element("sql").Value; }
        }

        protected virtual IParameter[] GetSqlParameters(ISqlHelper sqlHelper)
        {
            return ViewElement.Elements("parameter")
                .Select(x => sqlHelper.CreateParameter(x.GetAttribute("name"),ValueGetter.GetFromValueGetter(x)))
                .ToArray();
        }

        protected override DataTable GetDataTable()
        {  
            var dsn = TableRepository.GetNearestConnectionString(ViewElement);
            using (ISqlHelper sqlHelper = DataLayerHelper.CreateSqlHelper(dsn))
            {
                IRecordsReader reader = sqlHelper.ExecuteReader(SQL, GetSqlParameters(sqlHelper));
                return reader.GetDataTable();                 
            }
        }
    }
}