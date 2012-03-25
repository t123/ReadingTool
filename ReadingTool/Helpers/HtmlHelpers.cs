#region License
// HtmlHelpers.cs is part of ReadingTool
// 
// ReadingTool is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;
using ReadingTool.Common;
using ReadingTool.Common.Attributes;
using ReadingTool.Common.Helpers;
using ReadingTool.Entities;
using ReadingTool.Models.View.Word;

namespace ReadingTool.Helpers
{
    public static class HtmlFormatters
    {
        public static string FormatTimespan(this HtmlHelper html, TimeSpan timespan)
        {
            return Formatters.FormatTimespan(timespan);
        }

        public static string FormatDate(this HtmlHelper html, DateTime date, string customFormat)
        {
            return date.ToString(customFormat);
        }

        public static string FormatDate(this HtmlHelper html, DateTime? date, string customFormat)
        {
            return date.HasValue ? FormatDate(html, date.Value, customFormat) : "NA";
        }

        public static string FormatDate(this HtmlHelper html, DateTime date)
        {
            return date.ToString(SystemSettings.Instance.Values.Formats.LongDateFormat);
        }

        public static string FormatDate(this HtmlHelper html, DateTime? date)
        {
            return date.HasValue ? FormatDate(html, date.Value) : "NA";
        }

        public static string FormatDateTime(this HtmlHelper html, DateTime date)
        {
            return date.ToString(
                SystemSettings.Instance.Values.Formats.LongDateFormat +
                " " +
                SystemSettings.Instance.Values.Formats.Time24Format
            );
        }

        public static string FormatDateTime(this HtmlHelper html, DateTime? date)
        {
            return date.HasValue ? FormatDateTime(html, date.Value) : "NA";
        }

        public static MvcHtmlString FormatSentence(this HtmlHelper html, WordViewModel word)
        {
            return new MvcHtmlString(FormatSentence(word.Sentence, word.FullDefinition, word.WordPhrase));
        }

        public static MvcHtmlString FormatSentence(this HtmlHelper html, Word word)
        {
            return new MvcHtmlString(FormatSentence(word.Sentence, word.FullDefinition, word.WordPhrase));
        }

        private static string FormatSentence(string sentence, string definition, string word)
        {
            if(word == null || string.IsNullOrEmpty(sentence)) return string.Empty;

            sentence = HttpUtility.HtmlEncode(sentence);
            string encodedWord = HttpUtility.HtmlEncode(word);

            if(!string.IsNullOrEmpty(definition))
            {
                sentence = sentence.Replace(encodedWord, string.Format(@"<a class=""s"" title=""{0}"">{1}</a>", HttpUtility.HtmlEncode(definition), encodedWord));
            }
            else
            {
                sentence = sentence.Replace(encodedWord, string.Format(@"<strong><u>{0}</u></strong>", encodedWord));
            }

            return sentence;
        }
    }

    public static class HtmlHelpers
    {
        /// <summary>
        /// http://craftingsoft.wordpress.com/2011/04/23/applying-unobtrusive-jquery-validation-to-dynamic-content-in-asp-net-mvc/
        /// </summary>
        public static void RegisterFormContextForValidation(this HtmlHelper helper)
        {
            if(helper.ViewContext.FormContext == null)
            {
                helper.ViewContext.FormContext = new FormContext();
            }
        }

        #region label extensions
        public static MvcHtmlString LabelHelpFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            return LabelHelpFor(html, expression, new RouteValueDictionary(new object()));
        }

        public static MvcHtmlString LabelHelpFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes)
        {
            return LabelHelpFor(html, expression, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString LabelHelpFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IDictionary<string, object> htmlAttributes)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            string htmlFieldName = ExpressionHelper.GetExpressionText(expression);

            string labelText = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();

            if(string.IsNullOrEmpty(labelText))
            {
                return MvcHtmlString.Empty;
            }

            Type type = typeof(TModel);
            MemberExpression memberExpression = (MemberExpression)expression.Body;
            string propertyName = ((memberExpression.Member is PropertyInfo) ? memberExpression.Member.Name : null);

            HelpAttribute attr = (HelpAttribute)type.GetProperty(propertyName).GetCustomAttributes(typeof(HelpAttribute), true).SingleOrDefault();

            if(attr == null)
            {
                MetadataTypeAttribute metadataType = (MetadataTypeAttribute)type.GetCustomAttributes(typeof(MetadataTypeAttribute), true).FirstOrDefault();
                if(metadataType != null)
                {
                    var property = metadataType.MetadataClassType.GetProperty(propertyName);
                    if(property != null)
                    {
                        attr = (HelpAttribute)property.GetCustomAttributes(typeof(HelpAttribute), true).SingleOrDefault();
                    }
                }
            }

            string helpText = attr != null ? attr.Description : string.Empty;

            TagBuilder label = new TagBuilder("label");
            label.MergeAttributes(htmlAttributes);
            label.Attributes.Add("for", html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName));

            if(string.IsNullOrEmpty(helpText))
            {
                label.SetInnerText(labelText);
                return new MvcHtmlString(label.ToString());
            }

            TagBuilder div = new TagBuilder("div");
            div.AddCssClass("formhelp");
            div.InnerHtml = helpText;

            TagBuilder a = new TagBuilder("a");
            a.Attributes.Add("class", "help");
            a.SetInnerText("?");

            label.InnerHtml = labelText + a.ToString() + div.ToString();

            return new MvcHtmlString(label.ToString());
        }

        public static MvcHtmlString LabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes)
        {
            return LabelFor(html, expression, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString LabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IDictionary<string, object> htmlAttributes)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
            string labelText = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();

            if(string.IsNullOrEmpty(labelText))
            {
                return MvcHtmlString.Empty;
            }

            TagBuilder tag = new TagBuilder("label");
            tag.MergeAttributes(htmlAttributes);
            tag.Attributes.Add("for", html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName));
            tag.SetInnerText(labelText);

            return MvcHtmlString.Create(tag.ToString(TagRenderMode.Normal));
        }
        #endregion

        /// <summary>
        /// http://haacked.com/archive/2011/03/05/defining-default-content-for-a-razor-layout-section.aspx
        /// </summary>
        /// <param name="webPage"></param>
        /// <param name="name"></param>
        /// <param name="defaultContents"></param>
        /// <returns></returns>
        public static HelperResult RenderSection(this WebPageBase webPage, string name, Func<dynamic, HelperResult> defaultContents)
        {
            if(webPage.IsSectionDefined(name))
            {
                return webPage.RenderSection(name);
            }
            return defaultContents(null);
        }

        #region cycle helper
        public static string Cycle(this HtmlHelper html, params string[] strings)
        {
            var context = html.ViewContext.HttpContext;
            int index = Convert.ToInt32(context.Items["cycle_index"]);

            string returnValue = strings[index % strings.Length];

            html.ViewContext.HttpContext.Items["cycle_index"] = ++index;
            return returnValue;
        }
        #endregion
    }
}