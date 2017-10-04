using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Eksponent.Dewd.Editors;
using umbraco.DataLayer;
using Eksponent.Dewd.Extensions;
using System.Text;
using System.Data;
using Eksponent.Dewd.Fields;

namespace Eksponent.Dewd.Repositories.Table
{
    public class TableLinkingField : Field
    {
        private XElement linkingElement;

        public TableLinkingField(XElement fieldElement, IEditor editor)
            : base(fieldElement, editor)
        {
            linkingElement = Xml.Element("linkingField");
            Editor.AfterSave += Editor_AfterSave;
            Editor.BeforeDelete += Editor_BeforeDelete;
        }

        protected new TableEditor Editor
        {
            get
            {
                return (TableEditor)base.Editor;
            }
        }

        void Editor_BeforeDelete(object sender, RowEventArgs e)
        {
            using (ISqlHelper sqlHelper = DataLayerHelper.CreateSqlHelper(Editor.GetConnectionString()))
            {
                var sql = String.Format("DELETE FROM {0} WHERE {1}=@LocalID",
                    linkingElement.GetAttribute("linkingTable"),
                    linkingElement.GetAttribute("localKey"));
                sqlHelper.ExecuteNonQuery(sql, new IParameter[] { sqlHelper.CreateParameter("@LocalID", e.ID.Value) });
            }
        }

        void Editor_AfterSave(object sender, AfterSaveEventArgs e)
        {
            if (e.ID == null)
                return; // no ID, error must have occured, so do nothing

            // note: would be nice to wrap this in a transaction, but isn't possible with umb datalayer
            var val = e.Values[UniqueID];

            using (ISqlHelper sqlHelper = DataLayerHelper.CreateSqlHelper(Editor.GetConnectionString()))
            {
                // remove existing
                var sql = String.Format("DELETE FROM {0} WHERE {1}=@LocalID",
                    linkingElement.GetAttribute("linkingTable"),
                    linkingElement.GetAttribute("localKey"));
                sqlHelper.ExecuteNonQuery(sql, new IParameter[] { sqlHelper.CreateParameter("@LocalID", e.ID.Value) });
                Trace.Info("TableLinkingField.Remove {0}", sql);

                // insert new
                sql = String.Format("INSERT INTO {0} ({1},{2}) VALUES (@LocalID,@ForeignID)",
                    linkingElement.GetAttribute("linkingTable"),
                    linkingElement.GetAttribute("localKey"),
                    linkingElement.GetAttribute("foreignKey"));
                Trace.Info("TableLinkingField.Insert {0}", sql);
                var ids = String.Format("{0}", val).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string id in ids)
                {
                    sqlHelper.ExecuteNonQuery(sql,
                        new IParameter[] {
                            sqlHelper.CreateParameter("@LocalID", e.ID.Value),
                            sqlHelper.CreateParameter("@ForeignID", id)
                        });
                }
            }
        }

        private XElement LinkingElement
        {
            get
            {
                return linkingElement;
            }
        }

        public override object RetrieveValue(RowID id)
        {
            var ids = new List<string>();
            if (id == null)
                return ids.ToArray();

            using (ISqlHelper sqlHelper = DataLayerHelper.CreateSqlHelper(Editor.GetConnectionString()))
            {
                var sql = String.Format("SELECT {0} FROM {1} WHERE {2}=@LocalID",
                    linkingElement.GetAttribute("foreignKey"),
                    linkingElement.GetAttribute("linkingTable"),
                    linkingElement.GetAttribute("localKey"));
                Trace.Info("TableLinkingField.RetrieveValue {0}", sql);
                IRecordsReader reader = sqlHelper.ExecuteReader(sql,
                    new IParameter[] { sqlHelper.CreateParameter("@LocalID", id.Value) });
                foreach (IDataRecord record in reader)
                    ids.Add(String.Format("{0}", record.GetValue(0)));
            }
            var idStr = String.Join(",", ids.ToArray());
            Trace.Info("TableLinkingField.RetrieveValue values {0}", idStr);
            return idStr;
        }
    }
}