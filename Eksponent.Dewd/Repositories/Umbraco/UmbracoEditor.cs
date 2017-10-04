using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Eksponent.Dewd.Editors;
using umbraco.cms.businesslogic.web;
using Eksponent.Dewd.Fields;

namespace Eksponent.Dewd.Repositories.Umbraco
{
    public class UmbracoEditor : Editor
    {
        public UmbracoEditor(XElement element, IRepository repository)
            : base(element, repository)
        {
        }

        protected override Dictionary<string, object> GetRowValues(RowID id)
        {
            var vals = new Dictionary<string, object>();
            if (id == null)
                return vals;

            var d = new Document((int)id.Value);
            var selectFields = SourceFields.Select(f => new { Field = f, SourceField = ((IHasSourceField)f).SourceField });
            foreach (var dbField in selectFields)
            {
                var prop = d.getProperty(dbField.SourceField);
                object val;
                if (prop == null)
                {
                    switch (dbField.SourceField)
                    {
                        case "SysId": val = d.Id; break;
                        case "SysCreateDateTime": val = d.CreateDateTime; break;
                        case "SysImage": val = d.Image; break;
                        case "SysText": val = d.Text; break;
                        case "SysUpdateDate": val = d.UpdateDate; break;
                        default:
                            throw new FormattedException("Property not found: {0} (from sourceField attribute).", dbField.SourceField);
                    }
                }
                else
                    val = prop.Value;
                    
                vals.Add(dbField.Field.UniqueID, val);
            }
            return vals;
        }

        public override SaveResult ExecuteSave(ref RowID id, Dictionary<string, object> values)
        {
            if(id==null)
                return new SaveResult() { Success = false, ErrorMessage = "Creating new nodes/documents is not supported." };

            var d = new Document((int)id.Value);
            foreach(IField field in WriteableFields) {
                var sourceField = ((IHasSourceField)field).SourceField;
                var val = GetParameterValue(field, values);

                var prop = d.getProperty(sourceField);
                if (prop == null)
                {
                    switch (sourceField)
                    {
                        case "SysText": d.Text = (string)val; break;
                        case "SysUpdateDate": d.UpdateDate = (DateTime)val; break;
                        default:
                            throw new FormattedException(
                                "Property not found or is readonly: {0} (from sourceField attribute).", sourceField);
                    }
                }
                else
                    prop.Value = val;
            }

            return new SaveResult() { Success = true };
        }

        public override void ExecuteDelete(RowID id)
        {
            var d = new Document((int)id.Value);
            d.delete();
        }
    }
}