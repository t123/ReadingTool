using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using ReadingTool.Core.Attributes;

namespace ReadingTool.Site.Helpers
{
    public class BootstrapControlOptions
    {
        //TODO implement me
        public bool Label { get; set; }
        public bool ErrorClass { get; set; }
        public bool Prepend { get; set; }
        public bool Append { get; set; }
        public bool InlineValidation { get; set; }

        public static BootstrapControlOptions Default
        {
            get
            {
                return new BootstrapControlOptions()
                    {
                        Label = true,
                        ErrorClass = true,
                        Prepend = false,
                        Append = true,
                        InlineValidation = true
                    };
            }
        }
    }

    public static class BootstrapHelper
    {
        private static MvcHtmlString BootstrapControl<TModel, TValue>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression,
            BootstrapControlOptions options = null
            )
        {
            if(options == null)
            {
                options = BootstrapControlOptions.Default;
            }

            var metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
            string errorKey = string.IsNullOrEmpty(helper.ViewData.TemplateInfo.HtmlFieldPrefix)
                                  ? htmlFieldName
                                  : helper.ViewData.TemplateInfo.HtmlFieldPrefix + "." + htmlFieldName;

            var errorClass =
                helper.ViewData.ModelState.ContainsKey(errorKey) && helper.ViewData.ModelState[errorKey].Errors.Any()
                    ? "error"
                    : ""
                ;

            var tip = (TipAttribute)metadata.ContainerType.GetProperty(metadata.PropertyName).GetCustomAttributes(typeof(TipAttribute), true).FirstOrDefault();

            TagBuilder controlGroup = new TagBuilder("div");
            controlGroup.AddCssClass("control-group");

            if(!string.IsNullOrEmpty(errorClass))
            {
                controlGroup.AddCssClass(errorClass);
            }

            var label = helper.LabelFor(expression, new { @class = "control-label" });

            TagBuilder controls = new TagBuilder("div");
            controls.AddCssClass("controls");

            TagBuilder inputAppend = new TagBuilder("div");
            inputAppend.AddCssClass("input-append");
            inputAppend.InnerHtml = "{0}";

            TagBuilder helpInline = new TagBuilder("span");
            helpInline.AddCssClass("help-inline");
            helpInline.InnerHtml = helper.ValidationMessageFor(expression).ToString();

            string additional = string.Empty;
            if(metadata.ModelType != typeof(bool) && metadata.IsRequired)
            {
                TagBuilder isRequired = new TagBuilder("span");
                isRequired.AddCssClass("add-on tip");
                isRequired.Attributes.Add("ref", "tooltip");
                isRequired.Attributes.Add("title", "required field");

                TagBuilder i = new TagBuilder("i");
                i.AddCssClass("icon-star");
                i.InnerHtml = " ";

                isRequired.InnerHtml = i.ToString();
                additional += isRequired;
            }

            if(tip != null)
            {
                TagBuilder tipHtml = new TagBuilder("span");
                tipHtml.AddCssClass("add-on tip");
                tipHtml.Attributes.Add("ref", "tooltip");
                tipHtml.Attributes.Add("title", tip.Description);

                TagBuilder i = new TagBuilder("i");
                i.AddCssClass("icon-question-sign");
                i.InnerHtml = " ";

                tipHtml.InnerHtml = i.ToString();
                additional += tipHtml;
            }

            inputAppend.InnerHtml += additional;
            controls.InnerHtml = inputAppend.ToString() + helpInline.ToString();
            controlGroup.InnerHtml = label.ToString() + controls.ToString();

            return new MvcHtmlString(controlGroup.ToString());
        }

        public static MvcHtmlString BootstrapFormItem<TModel, TValue>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression
            )
        {
            var controlHtml = BootstrapControl(helper, expression, null);
            string control = string.Format(controlHtml.ToString(), helper.EditorFor(expression).ToString());
            return new MvcHtmlString(control);
        }

        public static MvcHtmlString BootstrapFormItem<TModel, TValue>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression,
            BootstrapControlOptions options
            )
        {
            throw new NotImplementedException();
            var controlHtml = BootstrapControl(helper, expression, options);
            string control = string.Format(controlHtml.ToString(), helper.EditorFor(expression).ToString());
            return new MvcHtmlString(control);
        }

        #region custom control html
        public static MvcHtmlString BootstrapFormCustomHtml<TModel, TValue>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression,
            string customHtml
            )
        {
            var controlHtml = BootstrapControl(helper, expression);
            string control = string.Format(controlHtml.ToString(), customHtml);
            return new MvcHtmlString(control);
        }

        public static MvcHtmlString BootstrapFormCustomHtml<TModel, TValue>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression,
            MvcHtmlString customHtml
            )
        {
            var controlHtml = BootstrapControl(helper, expression);
            string control = string.Format(controlHtml.ToString(), customHtml);
            return new MvcHtmlString(control);
        }
        #endregion

        #region textbox
        /// <summary>
        /// <seealso cref="http://stackoverflow.com/questions/11634117/stuck-on-razor-extension"/>
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="helper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static MvcHtmlString BootstrapFormTextbox<TModel, TValue>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression
            )
        {
            return BootstrapFormTextbox(helper, expression, new { });
        }

        public static MvcHtmlString BootstrapFormTextbox<TModel, TValue>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression,
            object htmlAttributes
            )
        {
            return BootstrapFormTextbox(helper, expression, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString BootstrapFormTextbox<TModel, TValue>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression,
            RouteValueDictionary htmlAttributes
            )
        {
            var controlHtml = BootstrapControl(helper, expression);
            string control = string.Format(controlHtml.ToString(), helper.TextBoxFor(expression, htmlAttributes).ToString());
            return new MvcHtmlString(control);
        }
        #endregion

        #region textarea
        public static MvcHtmlString BootstrapFormTextArea<TModel, TValue>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression
            )
        {
            return BootstrapFormTextArea(helper, expression, new { });
        }

        public static MvcHtmlString BootstrapFormTextArea<TModel, TValue>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression,
            object htmlAttributes
            )
        {
            return BootstrapFormTextArea(helper, expression, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString BootstrapFormTextArea<TModel, TValue>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression,
            RouteValueDictionary htmlAttributes
            )
        {
            var controlHtml = BootstrapControl(helper, expression);
            string control = string.Format(controlHtml.ToString(), helper.TextAreaFor(expression, htmlAttributes).ToString());
            return new MvcHtmlString(control);
        }
        #endregion

        #region dropdownlist
        public static MvcHtmlString BootstrapFormDropDown<TModel, TValue>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression,
            IEnumerable<SelectListItem> selectList
            )
        {
            return BootstrapFormDropDown(helper, expression, selectList, new { });
        }

        public static MvcHtmlString BootstrapFormDropDown<TModel, TValue>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression,
            IEnumerable<SelectListItem> selectList,
            object htmlAttributes
            )
        {
            return BootstrapFormDropDown(helper, expression, selectList, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString BootstrapFormDropDown<TModel, TValue>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression,
            IEnumerable<SelectListItem> selectList,
            RouteValueDictionary htmlAttributes
            )
        {
            return BootstrapFormDropDown(helper, expression, selectList, null, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString BootstrapFormDropDown<TModel, TValue>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression,
            IEnumerable<SelectListItem> selectList,
            string optionLabel
            )
        {
            var controlHtml = BootstrapControl(helper, expression);
            return BootstrapFormDropDown(helper, expression, selectList, optionLabel, new RouteValueDictionary());
        }

        public static MvcHtmlString BootstrapFormDropDown<TModel, TValue>(
            this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression,
            IEnumerable<SelectListItem> selectList,
            string optionLabel,
            RouteValueDictionary htmlAttributes
            )
        {
            var controlHtml = BootstrapControl(helper, expression);
            string control = string.Format(controlHtml.ToString(), helper.DropDownListFor(expression, selectList, optionLabel, htmlAttributes).ToString());
            return new MvcHtmlString(control);
        }
        #endregion
    }
}