using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ReadingTool.Core.Formatters;

namespace ReadingTool.Site.Helpers
{
    public static class DateHelper
    {
        public static string FormatTimespan(this HtmlHelper helper, TimeSpan timespan, string append = "", string prepend = "")
        {
            return timespan.ToHumanAgo(append: append, prepend: prepend);
        }

        public static string FormatDateHumanAgo(this HtmlHelper helper, DateTime date, string append = "", string prepend = "")
        {
            return date.ToHumanAgo(append: append, prepend: prepend);
        }

        public static string FormatDateHumanAgo(this HtmlHelper helper, DateTime? date, string append = "", string prepend = "")
        {
            return date.ToHumanAgo(append: append, prepend: prepend);
        }

        public static string FormatDate(this HtmlHelper helper, DateTime date, string customFormat)
        {
            return date.ToString(customFormat);
        }

        public static string FormatDate(this HtmlHelper helper, DateTime? date, string customFormat)
        {
            return date.HasValue ? FormatDate(helper, date.Value, customFormat) : "NA";
        }

        public static string FormatDate(this HtmlHelper helper, DateTime date)
        {
            return date.ToString("dd/MM/yy");
        }

        public static string FormatDate(this HtmlHelper helper, DateTime? date)
        {
            return date.HasValue ? FormatDate(helper, date.Value) : "NA";
        }

        public static string FormatDateTime(this HtmlHelper helper, DateTime date)
        {
            return date.ToString("dd/MM/yy H:mm:ss");
        }

        public static string FormatDateTime(this HtmlHelper helper, DateTime? date)
        {
            return date.HasValue ? FormatDateTime(helper, date.Value) : "NA";
        }
    }
}