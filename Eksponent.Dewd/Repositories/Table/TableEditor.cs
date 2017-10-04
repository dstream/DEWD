using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Eksponent.Dewd.Extensions;
using umbraco.DataLayer;
using Eksponent.Dewd.Editors;
using Eksponent.Dewd.Fields;

namespace Eksponent.Dewd.Repositories.Table
{
    public class TableEditor : Editor
    {
        protected string tableName;
        protected string primaryKeyName = "ID";

        public TableEditor(XElement editorElement, IRepository repository) : base(editorElement, repository)
        {
            tableName = editorElement.GetAttribute("tableName");
            primaryKeyName = repository.PrimaryKeyName;
        }

        public virtual string GetConnectionString()
        {
            return TableRepository.GetNearestConnectionString(editorElement);
        }

        public TableRepository TableRepository
        {
            get { return (TableRepository)repository; }
        }

        protected override object NullValue
        {
            get { return DBNull.Value; }
        }

        bool IsSqlCe(ISqlHelper sqlHelper)
        {
            return (sqlHelper.ToString() == "SqlCE4Umbraco.SqlCEHelper");
        }

        public virtual string GetSqlInsertStatement(ISqlHelper sqlHelper)
        {
            string lastIdFunction = "SELECT @@IDENTITY AS ID";
            if (sqlHelper is umbraco.DataLayer.SqlHelpers.MySql.MySqlHelper)
                lastIdFunction = "SELECT last_insert_id() AS ID";
            else if (IsSqlCe(sqlHelper))
                lastIdFunction = "";
            
            return "INSERT INTO {0} ({1}) VALUES ({2}); "+lastIdFunction;
        }

        public virtual string GetSqlUpdateStatement(ISqlHelper sqlHelper)
        {
            return "UPDATE {0} SET {1} WHERE {2}=@ID";
        }

        public virtual string GetSqlDeleteStatement(ISqlHelper sqlHelper)
        {
            return "DELETE FROM {0} WHERE {1}=@ID";
        }

        public override SaveResult ExecuteSave(ref RowID id, Dictionary<string, object> values)
        {
            using (ISqlHelper sqlHelper = DataLayerHelper.CreateSqlHelper(GetConnectionString()))
            {
                int pIndex = 0;

                var parameters = WriteableFields.Select(f =>
                    new
                    {
                        SourceField = ((IHasSourceField)f).SourceField,
                        Parameter = sqlHelper.CreateParameter("@P" + (pIndex++), GetParameterValue(f, values))
                    }).ToList();

                string sql;
                var dbParams = new List<IParameter>(parameters.Select(p => p.Parameter));
                if (id == null)
                {
                    // todo: handle @@IDENTITY so it can be plugged for other databases
                    //sql = String.Format("INSERT INTO {0} ({1}) VALUES ({2}); SELECT @@IDENTITY AS ID",
                    sql = String.Format(GetSqlInsertStatement(sqlHelper), tableName,
                        String.Join(",", parameters.Select(p => p.SourceField).ToArray()),
                        String.Join(",", parameters.Select(p => p.Parameter.ParameterName).ToArray()));
                }
                else
                {
                    sql = String.Format(GetSqlUpdateStatement(sqlHelper), tableName,
                        String.Join(",", parameters.Select(p => p.SourceField + "=" + p.Parameter.ParameterName).ToArray()),
                        primaryKeyName);
                    dbParams.Add(sqlHelper.CreateParameter("@ID", id.Value));
                }

                Trace.Info("TableEditor.Save: Executing SQL: {0}", sql);
                foreach (IParameter param in dbParams)
                    Trace.Info("TableEditor.Save: ... with parameter {0} and value {1}", param.ParameterName, param.Value);

                // todo: put sql execution in virtual method (ExecuteSaveSql) which can be overridden
                IRecordsReader reader;
                try
                {
                    reader = sqlHelper.ExecuteReader(sql, dbParams.ToArray());
                    if (IsSqlCe(sqlHelper))
                    {
                        // hack: can't do multiple statements on CE, so this hack gets latest id (not safe)
                        if(!TableRepository.PrimaryKeyValueIsManual)
                            reader = sqlHelper.ExecuteReader(
                                String.Format("SELECT MAX({0}) AS ID FROM {1}", primaryKeyName, tableName));
                    } 
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                    if (ex.InnerException != null)
                        message = ex.InnerException.Message;
                    return new SaveResult() { Success = false, ErrorMessage = message };
                }

                // for new rows, determine the ID of the current new row
                if (id == null)
                {
                    if (TableRepository.PrimaryKeyValueIsManual)
                    {
                        // find value of primary key from sourceField name
                        var pm = parameters.FirstOrDefault(p => p.SourceField == primaryKeyName);
                        if (pm == null)
                            throw new FormattedException("Unable to locate field {0} to get primary key from.", primaryKeyName);
                        id = RowID.Get(String.Format("{0}", pm.Parameter.Value));
                    }
                    else
                    {
                        if (reader.Read())
                            id = RowID.Get(String.Format("{0}", reader.GetObject("ID")));
                        else
                            throw new FormattedException("Expected autogenerated primary key, but INSERT statement didn't return value in ID column.");
                    }
                    Trace.Info("TableEditor.Save: Inserted and ID {0} was returned", id.Value);
                }
            }
            return null; // defaults to SaveResult in Editor
        }

        public override void ExecuteDelete(RowID id)
        {
            using (ISqlHelper sqlHelper = DataLayerHelper.CreateSqlHelper(GetConnectionString()))
            {
                var sql = String.Format(GetSqlDeleteStatement(sqlHelper), tableName, primaryKeyName);
                sqlHelper.ExecuteNonQuery(sql, new IParameter[] { sqlHelper.CreateParameter("@ID", id.Value) });
            }
            Trace.Info("TableEditor.ExecuteDelete: Deleted row with ID {0}", id.Value);
        }

        protected override Dictionary<string, object> GetRowValues(RowID id)
        {
            var vals = new Dictionary<string, object>();
            if (id == null)
                return vals;

            var selectFields = SourceFields.Select(f => new { Field = f, SourceField = ((IHasSourceField)f).SourceField });
            
            var sql = String.Format("SELECT {0} FROM {1} WHERE {2}=@ID",
                String.Join(", ", selectFields.Select(s => String.Format("[{0}]", s.SourceField)).ToArray()),
                tableName,
                primaryKeyName);
            using (ISqlHelper sqlHelper = DataLayerHelper.CreateSqlHelper(GetConnectionString()))
            {
                Trace.Info(sql);
                IRecordsReader reader = sqlHelper.ExecuteReader(sql,
                    new IParameter[] { sqlHelper.CreateParameter("@ID", id.Value) });
                if (reader.Read())
                {
                    foreach (var dbField in selectFields)
                    {
                        vals.Add(dbField.Field.UniqueID, reader.GetObject(dbField.SourceField));
                    }
                }
            }

            return vals;
        }
    }
}