using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using umbraco.DataLayer;
using System.Reflection;

namespace Eksponent.Dewd.Extensions
{
    public static class DataLayerExtensions
    {
        public static DataTable GetDataTable(this IRecordsReader reader)
        {
            DataTable dt = null;
            object[] values;            
            foreach (IDataRecord record in reader)
            {
                if (dt == null)
                {
                    dt = new DataTable();
                    for (int i = 0; i < record.FieldCount; i++)
                    {
                        dt.Columns.Add(new DataColumn(record.GetName(i), record.GetFieldType(i)));
                    }
                }
                values = new object[record.FieldCount];
                // note: wierd problems with reading from sql when project was compiled in RELEASE mode
                record.GetValues(values);
                dt.Rows.Add(values);
            }
            return dt;
        }

        public static DataTable AsDataTable(this IEnumerable<object> list)
        {
            // convert object list to datatable
            DataTable dt = new DataTable();
            if(list.Count()==0)
                return dt;

            var lvType = list.First().GetType();
            PropertyInfo[] fiColl = lvType.GetProperties();
            Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();

            foreach (PropertyInfo fi in fiColl)
            {
                // skip any property which has the ExcludeDataTableSerializing-attribute
                //object[] attrs = fi.GetCustomAttributes(typeof(ExcludeDataTableSerializingAttribute), false);
                //if (attrs.Length > 0)
                //    continue;

                properties.Add(fi.Name, fi);
                if (!fi.PropertyType.IsGenericType)
                { // DataSet does not support Nullable<>
                    dt.Columns.Add(fi.Name, fi.PropertyType);
                }
                else
                {
                    if (fi.PropertyType.Name.StartsWith("Nullable"))
                    {
                        // convert nullable to underlying type
                        Type nulledType = fi.PropertyType.GetGenericArguments()[0];
                        dt.Columns.Add(fi.Name, nulledType);
                    }
                }
            }

            // transfer data to the datatable
            foreach (object obj in list)
            {
                DataRow row = dt.NewRow();
                foreach (DataColumn column in dt.Columns)
                {
                    object cellVal = properties[column.ColumnName].GetValue(obj, null);
                    row[column] = (cellVal == null ? DBNull.Value : cellVal);
                }

                dt.Rows.Add(row);
            }

            return dt;
        }
    }
}