using System.Xml.Linq;
using System.Collections.Generic;
using Eksponent.Dewd.Editors;
using Eksponent.Dewd.Repositories;
using Eksponent.Dewd;
using Eksponent.Dewd.Repositories.Object;

/// <summary>
/// Summary description for MyArEditor
/// </summary>
public class MyArEditor : Editor
{
    public MyArEditor(XElement element, IRepository repository)
        : base(element, repository)
    {
    }

    public override SaveResult ExecuteSave(ref RowID id, Dictionary<string, object> values)
    {
        // get instance, new or existing depending on ID value
        var ar = (id == null ? new MyActiveRecord() : MyActiveRecord.Get((int)id.Value));

        // set values on properties and commit changes
        ObjectUtil.SetPropertyValues(ar, values, Fields);
        ar.Save();

        // get the (new) id and return success status
        id = RowID.Get(ar.ID.Value);

        return new SaveResult() { Success = true };
    }

    public override void ExecuteDelete(RowID id)
    {
        // call delete method on instance to remove it
        MyActiveRecord.Get((int)id.Value).Delete();
    }

    protected override Dictionary<string, object> GetRowValues(RowID id)
    {
        // get instance of ID and retrieve values from properties
        var ar = MyActiveRecord.Get((int)id.Value);
        return ObjectUtil.GetPropertyValues(SourceFields, ar);
    }
}
