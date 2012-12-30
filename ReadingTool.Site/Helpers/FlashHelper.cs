using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ReadingTool.Site.Helpers
{
    public static class FlashHelper
    {
        public enum Level
        {
            Error,
            Alert,
            Info,
            Success
        }

        public class FlashMsg
        {
            public Level Level { get; set; }
            public string Message { get; set; }
            public long Ticks { get; private set; }

            /// <summary>
            /// Blank message at level INFO
            /// </summary>
            public FlashMsg()
                : this(Level.Info, "")
            {
            }

            /// <summary>
            /// Message at level INFO with message MESSAGE
            /// </summary>
            /// <param name="message"></param>
            public FlashMsg(string message)
                : this(Level.Info, message)
            {
            }

            /// <summary>
            /// Message at level LEVEL with message string.Format(MESSAGE, inject)
            /// </summary>
            /// <param name="level"></param>
            /// <param name="message"></param>
            /// /// <param name="inject">The parameters you want to inject into message</param>
            public FlashMsg(Level level, string message, params object[] inject)
            {
                Level = level;
                Message = string.Format(message, inject);
                Ticks = DateTime.Now.Ticks;
            }
        }

        /// <summary>
        /// A new flash message at the INFO level
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="message"></param>
        /// <param name="inject">The parameters you want to inject into message</param>
        public static void FlashInfo(this Controller controller, string message, params object[] inject)
        {
            FlashMessage(controller, new FlashMsg(Level.Info, message, inject));
        }

        /// <summary>
        /// A new flash message at the ALERT level
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="message"></param>
        /// <param name="inject">The parameters you want to inject into message</param>
        public static void FlashAlert(this Controller controller, string message, params object[] inject)
        {
            FlashMessage(controller, new FlashMsg(Level.Alert, message, inject));
        }

        /// <summary>
        /// A new flash message at the ERROR level
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="message"></param>
        /// <param name="inject">The parameters you want to inject into message</param>
        public static void FlashError(this Controller controller, string message, params object[] inject)
        {
            FlashMessage(controller, new FlashMsg(Level.Error, message, inject));
        }

        /// <summary>
        /// A new flash message at the SUCCESS level
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="message"></param>
        /// <param name="inject">The parameters you want to inject into message</param>
        public static void FlashSuccess(this Controller controller, string message, params object[] inject)
        {
            FlashMessage(controller, new FlashMsg(Level.Success, message, inject));
        }

        /// <summary>
        /// A new flash message at the LEVEL level
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="level">The level</param>
        /// <param name="message">The message</param>
        /// <param name="inject">The parameters you want to inject into message</param>
        public static void FlashMessage(this Controller controller, Level level, string message, params object[] inject)
        {
            FlashMessage(controller, new FlashMsg(level, message, inject));
        }

        /// <summary>
        /// A new flash message 
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="message"></param>
        public static void FlashMessage(this Controller controller, FlashMsg message)
        {
            IList<FlashMsg> messages = controller.TempData["___FLASH_MESSAGES___"] as IList<FlashMsg>;

            if(messages == null)
            {
                messages = new List<FlashMsg>() { message };
            }
            else
            {
                messages.Add(message);
            }

            controller.TempData["___FLASH_MESSAGES___"] = messages;
        }

        /// <summary>
        /// Renders all the flash message(s) as HTML
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public static HtmlString Flash(this HtmlHelper helper)
        {
            IList<FlashMsg> messages = helper.ViewContext.TempData["___FLASH_MESSAGES___"] as IList<FlashMsg>;

            if(messages == null || messages.Count == 0)
            {
                return new MvcHtmlString("");
            }

            var clr10 = new TagBuilder("div");
            clr10.AddCssClass("clr10");

            StringBuilder sb = new StringBuilder(clr10.ToString());
            foreach(var grouping in messages.OrderBy(x => x.Level).ThenBy(x => x.Ticks).GroupBy(x => x.Level))
            {
                foreach(var flashMessage in grouping)
                {
                    sb.Append(ConstructHtml(flashMessage.Message, flashMessage.Level, htmlEncode: false));
                }
            }

            sb.Append(clr10.ToString());
            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString FlashMessage(this HtmlHelper helper, string message, Level level = Level.Info, bool htmlEncode = true)
        {
            return ConstructHtml(message, level, false, htmlEncode);
        }

        private static MvcHtmlString ConstructHtml(string message, Level level, bool includeClose = true, bool htmlEncode = true)
        {
            TagBuilder div = new TagBuilder("div");
            div.AddCssClass("flash-message");
            div.AddCssClass("alert");
            div.AddCssClass("alert-" + level.ToString().ToLowerInvariant());

            if(includeClose)
            {
                TagBuilder a = new TagBuilder("a");
                a.AddCssClass("close");
                a.Attributes.Add("href", "#");
                a.Attributes.Add("onclick", "$(this).parent('.flash-message').fadeOut(); return false;");
                a.InnerHtml = "&times";

                div.InnerHtml = a.ToString();
            }

            TagBuilder p1 = new TagBuilder("p");
            TagBuilder p2 = new TagBuilder("p");
            TagBuilder span = new TagBuilder("span");

            if(htmlEncode)
            {
                span.SetInnerText(message);
            }
            else
            {
                span.InnerHtml = message;
            }

            p2.InnerHtml = span.ToString();
            div.InnerHtml += p1.ToString() + p2.ToString();

            return new MvcHtmlString(div.ToString());
        }
    }
}