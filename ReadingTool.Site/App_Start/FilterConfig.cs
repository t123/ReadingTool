using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace ReadingTool.Site
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //filters.Add(new HandleErrorAttribute()
            //    {
            //        ExceptionType = typeof(Exception),
            //        View = "~/Views/Error/Index.cshtml"
            //    });
        }
    }
}