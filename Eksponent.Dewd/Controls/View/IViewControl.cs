using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Eksponent.Dewd.Repositories;
using Eksponent.Dewd.Views;

namespace Eksponent.Dewd.Controls.View
{
    /// <summary>
    /// Describes the methods a web control must implement to be used to render rows for an IView. 
    /// View controls should (if possible) be data source neutral.
    /// </summary>
    public interface IViewControl
    {
        /// <summary>
        /// Gets or sets the data source.
        /// </summary>
        /// <value>The data source.</value>
        object DataSource { get; set; }

        /// <summary>
        /// Binds the data provided in the DataSource field. If the DataSource is null or doesn't 
        /// contain any rows, the IViewControl should display a value like "No rows to display in the current view."
        /// </summary>
        void DataBind();

        /// <summary>
        /// Gets or sets a value indicating whether editing is available.
        /// </summary>
        /// <value><c>true</c> if editing is enabled; otherwise, <c>false</c>.</value>
        bool EnableEdit { get; set; }

        /// <summary>
        /// Gets or sets the repository.
        /// </summary>
        /// <value>The repository.</value>
        IRepository Repository { get; set; }

        /// <summary>
        /// Gets or sets the view.
        /// </summary>
        /// <value>The view.</value>
        IView View { get; set; }
    }
}