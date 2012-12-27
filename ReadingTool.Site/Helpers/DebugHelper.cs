using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReadingTool.Site.Helpers
{
    public static class DebugHelper
    {
        public static bool IsDebug(this HtmlHelper helper)
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}