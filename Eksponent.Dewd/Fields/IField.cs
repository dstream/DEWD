using System;
using System.Xml.Linq;
using Eksponent.Dewd.Editors;
using Eksponent.Dewd.Controls;

namespace Eksponent.Dewd.Fields
{
    /// <summary>
    /// Represents a field which is the link between editor controls and a specific column/property on a row 
    /// in the data source. Should as far as possible be data source neutral. Use Field as base type for your 
    /// own implementations.
    /// </summary>
    public interface IField : IEquatable<IField>
    {
        /// <summary>
        /// Must return an ID which can be used to identify the field uniquely (reaaally important!).
        /// </summary>
        string UniqueID { get; }

        /// <summary>
        /// Should return full field definition as xml element.
        /// </summary>
        XElement Xml { get; }

        /// <summary>
        /// May be used by fields which aren't data source neutral to retrieve the value 
        /// from the underlying data source.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        object RetrieveValue(RowID id);

        /// <summary>
        /// Called right before the field value is handed our to the umbraco editor control for editing.
        /// </summary>
        /// <param name="value">Value from data source</param>
        void ProcessValueBeforeEdit(ref object value);
        
        /// <summary>
        /// Called right before the field value is persisted in the data source.
        /// </summary>
        /// <param name="value">Value retrieved from the umbraco editor control.</param>
        void ProcessValueBeforeSave(ref object value);

        /// <summary>
        /// Gets the content control definition. Should return null if field doesn't support user editing.
        /// </summary>
        /// <value>The content control definition.</value>
        ContentControlDefinition ContentControlDefinition { get; }
    }
}
