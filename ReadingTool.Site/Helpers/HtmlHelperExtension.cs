#region License
// HtmlHelperExtension.cs is part of ReadingTool.Site
// 
// ReadingTool.Site is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Site is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Site. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2013 Travis Watt
#endregion

using System;
using System.Linq;
using System.Linq.Expressions;
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