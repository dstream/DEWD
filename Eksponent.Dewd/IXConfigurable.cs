using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Eksponent.Dewd
{
    public interface IXConfigurable
    {
        XElement ConfigurationElement { get; set; }
    }
}