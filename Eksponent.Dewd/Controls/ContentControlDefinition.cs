using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;

namespace Eksponent.Dewd.Controls
{
    /// <summary>
    /// ContentControlDefinition describes the content controls (fields) used with ContentControlContainer.
    /// </summary>
    public class ContentControlDefinition
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string TabTitle { get; set; }
        public object InitialValue { get; set; }
        public DataTypeDefinition DataTypeDefinition { get; set; }
        public InitializeTypeInstanceDelegate InitializeTypeInstanceCallBack { get; set; }
        public string Caption { get; set; }

        public ContentControlDefinition()
        {
        }
    }

    public delegate void InitializeTypeInstanceDelegate(IDataType dataTypeInstance);
}