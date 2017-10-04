using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.UI;
using Eksponent.Dewd.Controls.View;

namespace Eksponent.Dewd.Views
{
    /// <summary>
    /// Encapsulates functionality for retrieving and displaying data from the data source.
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// Returns an enumerable object or data table which can be binded to a grid.
        /// </summary>
        /// <returns></returns>
        object GetData();

        /// <summary>
        /// Returns the name of the view.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Creates a new instance of the web control which renders the data list. The control 
        /// is added to the page control collection by the View.aspx page.
        /// </summary>
        IViewControl CreateViewControl();

        /// <summary>
        /// Provides access to the IViewControl which renders the data list.
        /// </summary>
        IViewControl ViewControl { get; }
    }   
}