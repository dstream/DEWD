using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for MyActiveRecord
/// </summary>
public class MyActiveRecord
{
    public int? ID { get; private set; }
    public string Property1 { get; set; }
    public string Property2 { get; set; }
    public DateTime DateProperty { get; set; }

	public MyActiveRecord()
	{
	}

    public static List<MyActiveRecord> All {
        get {
            var list = HttpContext.Current.Session["MyActiveRecordDb"] as List<MyActiveRecord>;
            if(list!=null)
                return list;

            HttpContext.Current.Session["MyActiveRecordDb"] = list = new List<MyActiveRecord>();
            for(int i=1; i<50; i++) 
            {
                list.Add(new MyActiveRecord() { 
                    ID = i, 
                    Property1 = String.Format("Hello {0} from Property1", i), 
                    Property2 = String.Format("Hello {0} from Property2", i)
                });
            };
            return list;
        }
    }

    public static MyActiveRecord Get(int id) {
        return All.First(ar => ar.ID == id);
    }

    public static IEnumerable<MyActiveRecord> GetAll()
    {
        return All;
    }

    public static IEnumerable<MyActiveRecord> GetAll(int howMany)
    {
        return All.Take(howMany);
    }

    public void Save() {
        // changes are made directly to object, so we don't have to do anything unless it's a new record
        if (ID == null)
        {
            ID = All.Max(ar => ar.ID) + 1;
            All.Add(this);
        }
    }

    public void Delete() {
        All.Remove(this);
    }
}