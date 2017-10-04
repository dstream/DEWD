using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Eksponent.Dewd.Views;
using Eksponent.Dewd.Repositories;

namespace Eksponent.Dewd.Controls.View.Columns
{
    public interface IRequiresContext
    {
        IView ViewContext { get; set; }
        IRepository RepositoryContext { get; set; }
    }
}