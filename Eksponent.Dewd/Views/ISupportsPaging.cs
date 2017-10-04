using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using System.Data;

namespace Eksponent.Dewd.Views
{
    public interface ISupportsPaging
    {
        /// <summary>
        /// GetData should retrieve a limited number of rows starting a the given offset from the underlying datasource.
        /// </summary>
        /// <param name="offset">Zero-based offset (0 = first row).</param>
        /// <param name="count">Number of rows to retrieve from the data source.</param>
        /// <returns></returns>
        object GetData(int offset, int count);

        /// <summary>
        /// GetTotalCount should get the total number of rows in the underlying data source.
        /// </summary>
        /// <returns></returns>
        int GetTotalCount();

        /// <summary>
        /// PageSize should return the number of rows on each page.
        /// </summary>
        int PageSize { get; }

        /// <summary>
        /// Instructs the user interface to enable or disable the pager controls.
        /// </summary>
        bool ShowPager { get; }
    }
}