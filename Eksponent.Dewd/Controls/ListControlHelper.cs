using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Eksponent.Dewd.Extensions;
using Eksponent.Dewd.Repositories.Table;

namespace Eksponent.Dewd.Controls
{
    public static class ListControlHelper
    {
        public static ListItem[] GetItemsFromConfiguration(XElement fieldElement)
        {
            var items = new List<ListItem>(); 
            var ctlConfig = fieldElement.Element("listControlDataSource");
            if (ctlConfig != null)
            {
                if (ctlConfig.GetAttribute<bool>("includeEmpty"))
                    items.Add(new ListItem("Select ...", ctlConfig.GetAttribute("emptyValue", "")));

                /*
                var dsType = Configuration.GetCustomType(
                    ctlConfig.GetAttribute("type"),
                    typeof(TableListControlDataSource), // todo: make this more generic .. somehow
                    new string[] { "Eksponent.Dewd.Controls" });
                var dataSource = (IListControlDataSource)Activator.CreateInstance(dsType);*/
                var dataSource = TypeInstantiater.GetInstance<IListControlDataSource>(
                    ctlConfig.GetAttribute("type"),
                    typeof(TableListControlDataSource));
                items.AddRange(dataSource.GetListItems(ctlConfig));
            }
            return items.ToArray();
        }
    }
}