using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Eksponent.Dewd
{
    /// <summary>
    /// Represents a generic row identifier which can be either integer og guid based.
    /// </summary>
    public class RowID
    {
        private object id;

        public static RowID Get(int id)
        {
            return new RowID() { id = id };
        }

        public static RowID Get(string value)
        {
            if (String.IsNullOrEmpty(value))
                return null;

            int intVal;
            if (Int32.TryParse(value, out intVal))
                return new RowID() { id = intVal }; // int32 based

            Guid guidVal;
            try
            {
                guidVal = new Guid(value);
                return new RowID() { id = guidVal }; // guid based
            }
            catch
            {
            }
            //if (Guid.TryParse(value, out guidVal))

            return new RowID() { id = value }; // string based
        }

        public object Value { get { return id; } }
    }
}