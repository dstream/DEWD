using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Eksponent.Dewd.Fields
{
    /// <summary>
    /// Allows a field to provide a default value.
    /// </summary>
    public interface ISupportsDefaultValue
    {
        /// <summary>
        /// Default value which will be set initially when a new row is edited.
        /// </summary>
        object DefaultValue { get; }

        /// <summary>
        /// Value that triggers the default value to be set again (if HasDefaultTrigger returns true).
        /// </summary>
        bool TriggersDefault(object value);
    }
}