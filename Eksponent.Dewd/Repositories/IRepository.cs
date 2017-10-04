using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Eksponent.Dewd.Views;
using Eksponent.Dewd.Editors;

namespace Eksponent.Dewd.Repositories
{
    public interface IRepository
    {
        string Name { get; }
        List<IView> Views { get; }
        IEditor Editor { get; }
        string PrimaryKeyName { get; }
    }
}