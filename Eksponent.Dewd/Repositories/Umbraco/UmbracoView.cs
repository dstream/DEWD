using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Eksponent.Dewd.Views;
using Eksponent.Dewd.Extensions;
using Eksponent.Dewd.Fields.ValueGetters;
using umbraco.cms.businesslogic.web;
using System.Data;
using umbraco.cms.businesslogic.property;

namespace Eksponent.Dewd.Repositories.Umbraco
{
    public class UmbracoView : DataTableBasedView
    {
        public UmbracoView(XElement viewElement, IRepository repository)
            : base(viewElement, repository)
        {
        }

        protected override DataTable GetDataTable()
        {
            var axis = ViewElement.GetAttribute("axis", "children");
            var nodeId = ViewElement.GetAttribute<int>("nodeId", 0);
            var docTypes = ViewElement.GetAttribute("documentTypes", "")
                .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

            var dt = new DataTable();
            var root = new Document(nodeId);

            // generate list of relevant documents
            var list = new List<Document>();
            if (axis == "children")
                list = root.Children.ToList();
            else if (axis.StartsWith("descendants"))
            {
                var stack = new Stack<Document>();
                stack.Push(root);
                do
                {
                    var doc = stack.Pop();
                    list.Add(doc);    
                    foreach(Document child in doc.Children)
                        stack.Push(child);
                } while (stack.Count != 0);
                if (!axis.EndsWith("-or-self"))
                    list.RemoveAt(0);
            }
            else throw new FormattedException("Unknown axis type: {0}", axis);

            // add system properties
            dt.Columns.Add(new DataColumn("SysId") { Caption = "ID" });
            dt.Columns.Add(new DataColumn("SysCreateDateTime") { Caption = "Created" });
            dt.Columns.Add(new DataColumn("SysImage") { Caption = "Icon" });            
            dt.Columns.Add(new DataColumn("SysText") { Caption = "Name" });            
            dt.Columns.Add(new DataColumn("SysUpdateDate") { Caption = "Updated" });            

            // generate data table from doc list
            foreach (Document child in list)
            {
                if (docTypes.Count != 0) // if none, grab all.. else:
                    if (!docTypes.Contains(child.ContentType.Alias))
                        continue;

                var props = child.getProperties;

                // create header
                // todo: ignore docTypes, that has been encountered before
                foreach (Property prop in props)
                {
                    var colName = prop.PropertyType.Alias;
                    if (!dt.Columns.Contains(colName))
                        dt.Columns.Add(new DataColumn(colName) { Caption = prop.PropertyType.Name });
                }

                // insert data
                var dr = dt.NewRow();
                dr["SysId"] = child.Id;
                dr["SysCreateDateTime"] = child.CreateDateTime;
                dr["SysImage"] = child.Image;
                dr["SysText"] = child.Text;
                dr["SysUpdateDate"] = child.UpdateDate;
                foreach (Property prop in props)
                {
                    var colName = prop.PropertyType.Alias;
                    dr[colName] = prop.Value;
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }
   }
}