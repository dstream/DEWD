using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Eksponent.Dewd.Views;
using Eksponent.Dewd.Editors;
using Eksponent.Dewd.Repositories;

namespace Eksponent.Dewd
{
    public static class Context
    {
        public static IView View
        {
            get { return RequestTemp.Get<IView>("Context.View"); }
            internal set { RequestTemp.Put("Context.View", value); }
        }

        public static IEditor Editor
        {
            get { return RequestTemp.Get<IEditor>("Context.Editor"); }
            internal set { RequestTemp.Put("Context.Editor", value); }
        }

        public static IRepository Repository
        {
            get { return RequestTemp.Get<IRepository>("Context.Repository"); }
            internal set { RequestTemp.Put("Context.Repository", value); }
        }

        public static RowID RowID {
            get { return RequestTemp.Get<RowID>("Context.RowID"); }
            internal set { RequestTemp.Put("Context.RowID", value); }
        }
    }
}