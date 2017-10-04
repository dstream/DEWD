using System;
using Eksponent.Dewd.Fields;
using System.Xml.Linq;

namespace Eksponent.Dewd.Fields.ValueGetters
{
    public interface IValueGetter
    {
        object GetValue(XElement valueElement);
    }
}
