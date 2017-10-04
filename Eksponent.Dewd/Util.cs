using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco;

namespace Eksponent.Dewd
{
    public static class Util
    {
        public static Version UmbracoVersion
        {
            get
            {
                return new Version(GlobalSettings.CurrentVersion);
            }
        }

        public static IEnumerable<int> GetSequence(int start, int end)
        {
            for(var foo=start;foo<=end; foo++)
                yield return foo;
        }
    }
}