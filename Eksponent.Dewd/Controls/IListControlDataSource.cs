using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace Eksponent.Dewd.Controls
{
    /// <summary>
    /// Represents a ListControl data source which can deliver an array of 
    /// ListItems from an xml configuration element. 
    /// </summary>
    public interface IListControlDataSource
    {
        ListItem[] GetListItems(XElement listControlDataSourceElement);
    }
}