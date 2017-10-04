using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Eksponent.Dewd
{
    public interface IRequiresMenuButtons
    {
        /// <summary>
        /// Called when buttons should be added to the user interface.
        /// </summary>
        void AddMenuButtons();
    }
}