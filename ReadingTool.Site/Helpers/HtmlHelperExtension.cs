using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace ReadingTool.Site.Helpers
{
    public static class HtmlHelperExtension
    {
        public static string FormatYesNo(this HtmlHelper helper, bool value)
        {
            return value ? "Yes" : "No";
        }

        public static bool HasErrorFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression)
        {
            var htmlFieldName = ExpressionHelper.GetExpressionText(expression);
            ModelState modelState;
            if(helper.ViewData.ModelState.TryGetValue(htmlFieldName, out modelState))
            {
                if(modelState.Errors.Any())
                {
                    return true;
                }
            }

            return false;
        }
    }
}