using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Eksponent.Dewd.Views;
using Eksponent.Dewd.Extensions;
using Eksponent.Dewd.Fields.ValueGetters;

namespace Eksponent.Dewd.Repositories.Object
{
    public class ObjectView : DataTableBasedView
    {
        public ObjectView(XElement viewElement, IRepository repository)
            : base(viewElement, repository)
        {
        }

        protected override System.Data.DataTable GetDataTable()
        {
            var methodName = ViewElement.GetAttribute("selectMethod");
            if (methodName.Length == 0)
                throw new FormattedException("EnumerableView should have selectMethod attribute: ", ViewElement);

            var caller = new MethodCaller(methodName);

            var parameters = ViewElement.Elements("parameter")
                .Select(x => ValueGetter.GetFromValueGetter(x))
                .ToArray();

            return caller.Invoke<IEnumerable<object>>(parameters).AsDataTable();
        }
   }
}