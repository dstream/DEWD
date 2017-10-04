using System;
using System.Collections.Generic;
using Eksponent.Dewd;
using Eksponent.Dewd.Controls;

namespace Eksponent.Dewd.Editors
{
    /// <summary>
    /// Connects the editor web page with the data source and provides the fields, which are used to
    /// edit the content in the data source. Also responsible for retrieving and manupulating data on a single
    /// row/object in the data source.
    /// </summary>
    public interface IEditor
    {
        /// <summary>
        /// Returns a list of ContentControlDefinition specifying which fields should be displayed
        /// on the edit page including initial value for each field.
        /// </summary>
        /// <param name="id">ID of the row to edit.</param>
        /// <returns></returns>
        IEnumerable<ContentControlDefinition> GetContentControlDefinitions(RowID id);

        /// <summary>
        /// Saves the field values back to the data source using the specified ID.
        /// </summary>
        /// <param name="id">The ID of the row to update.</param>
        /// <param name="values">The values. Key of the dictionary should be correspond to IField.UniqueID.</param>
        /// <returns></returns>
        SaveResult Save(ref RowID id, Dictionary<string, object> values);

        /// <summary>
        /// Deletes a row using the specified ID.
        /// </summary>
        /// <param name="id">The ID of the row to delete.</param>
        void Delete(RowID id);
    }
}
