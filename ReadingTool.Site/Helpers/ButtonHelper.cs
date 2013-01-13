using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using MongoDB.Bson;

namespace ReadingTool.Site.Helpers
{
    public static class ButtonHelper
    {
        #region delete button
        public static MvcHtmlString DeleteButton(this HtmlHelper html, string url, string classes = "")
        {
            return DeleteButton(html, url, null, classes);
        }

        public static MvcHtmlString DeleteButton(this HtmlHelper html, string url, ObjectId? id, string classes = "")
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(@"<form method=""post"" action=""{0}"" onsubmit=""return confirm('Are you sure you want to delete this?');"" style=""display:inline"">", url);
            sb.AppendFormat("{0}", html.AntiForgeryToken());
            if(id.HasValue)
            {
                sb.AppendFormat(@"<input type=""hidden"" name=""id"" value=""{0}"" />", id.Value);
            }

            sb.AppendFormat(@"<button type=""submit"" class=""btn btn-danger {0}"">delete</button>", classes);
            sb.Append("</form>");

            return new MvcHtmlString(sb.ToString());
        }
        #endregion

        #region edit button
        public static MvcHtmlString EditButton(this HtmlHelper html, string url, string classes = "")
        {
            return EditButton(html, url, null, classes);
        }

        public static MvcHtmlString EditButton(this HtmlHelper html, string url, ObjectId? id, string classes = "")
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(@"<a class=""btn {1}"" href=""{0}"" title=""edit"">edit</a>", url, classes);

            return new MvcHtmlString(sb.ToString());
        }
        #endregion

        #region generic button
        public static MvcHtmlString GenericButton(this HtmlHelper html, string label, string url, string classes)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"<a class=""btn {0}"" href=""{1}"" title=""{2}"">{2}</a>", classes, url, label);
            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString GenericFormButton(this HtmlHelper html, string label, string url, string classes)
        {
            return GenericFormButton(html, label, url, classes, null);
        }

        public static MvcHtmlString GenericFormButton(this HtmlHelper html, string label, string url, string classes, long? id)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(@"<form method=""post"" action=""{0}"" onsubmit=""return confirm('Are you sure you want to do this?');"">", url);
            sb.AppendFormat("{0}", html.AntiForgeryToken());
            if(id.HasValue)
            {
                sb.AppendFormat(@"<input type=""hidden"" name=""id"" value=""{0}"" />", id.Value);
            }

            sb.AppendFormat(@"<button type=""submit"" class=""btn {0}"">{1}</button>", classes, label);
            sb.Append("</form>");

            return new MvcHtmlString(sb.ToString());
        }
        #endregion
    }
}