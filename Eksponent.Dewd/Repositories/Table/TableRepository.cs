using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Eksponent.Dewd.Extensions;
using Eksponent.Dewd.Views;
using Eksponent.Dewd.Editors;
using umbraco;

namespace Eksponent.Dewd.Repositories.Table
{
    public class TableRepository : Repository
    {
        public TableRepository(XElement element) : base(element)
        {
        }

        protected override Type DefaultEditorType
        {
            get { return typeof(TableEditor); }
        }

        protected override Type DefaultViewType
        {
            get { return typeof(TableView); }
        }

        public static string GetNearestConnectionString(XElement element)
        {
            // look for dsn/connectionstring: editor -> repository -> container or return umbraco default
            var connStr = element.AncestorsAndSelf()
                .Select(e => e.GetAttribute("connection"))
                .FirstOrDefault(s => s.Length != 0);
            return connStr ?? GlobalSettings.DbDSN;
        }
    }
}