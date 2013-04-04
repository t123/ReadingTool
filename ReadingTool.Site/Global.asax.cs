#region License
// Global.asax.cs is part of ReadingTool.Site
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
using System.Net;
using System.Security.Principal;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Elmah;
using NHibernate.Event;
using ReadingTool.Common;
using ReadingTool.Entities;

namespace ReadingTool.Site
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        private log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const string SYSTEM_LANGUAGE_CACHE_KEY = @"SYSTEM_LANGUAGE_CACHE_KEY";

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            ModelBinders.Binders.DefaultBinder = new EmptyStringModelBinder();

            Mappings.Register();

            CacheSystemLanguages();
        }

        private void CacheSystemLanguages()
        {
            var systemLanguageRepository = DependencyResolver.Current.GetService<ReadingTool.Repository.Repository<SystemLanguage>>();
            var languages = systemLanguageRepository.FindAll().ToDictionary(x => x.Code, x => x.Name + " (" + x.Code + ")");

            HttpRuntime.Cache.Add(
                MvcApplication.SYSTEM_LANGUAGE_CACHE_KEY,
                languages,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                (key, value, reason) =>
                {
                });
        }

        void ErrorLog_Filtering(object sender, ExceptionFilterEventArgs e)
        {
            var exception = e.Exception.GetBaseException();
            var httpException = exception as HttpException;
            if(httpException != null)
            {
                if(httpException.GetHttpCode() == (int)HttpStatusCode.NotFound)
                {
                    string url = ((HttpContext)e.Context).Request.RawUrl;
                    string[] ignoreExtensions = new string[] { ".php", ".asp", ".aspx", ".gif", ".png", ".ico", ".woff" };

                    if(url.StartsWith("/apple-touch"))
                    {
                        e.Dismiss();
                    }

                    foreach(var ending in ignoreExtensions)
                    {
                        if(url.EndsWith(ending))
                        {
                            e.Dismiss();
                            return;
                        }
                    }
                }
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            try
            {
                HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

                if(authCookie != null)
                {
                    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                    IIdentity identity = HttpContext.Current.User.Identity;
                    UserPrincipal p = new UserPrincipal(new UserIdentity(
                                                            identity.Name,
                                                            identity.IsAuthenticated,
                                                            identity.AuthenticationType,
                                                            ServiceStack.Text.TypeSerializer.DeserializeFromString<UserIdentity.UserData>(authTicket.UserData)
                                                            ));
                    Context.User = p;
                    System.Threading.Thread.CurrentPrincipal = p;
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
                FormsAuthentication.SignOut();
            }
        }
    }

    internal class AuditEventListener : IPreInsertEventListener, IPreUpdateEventListener, IPreDeleteEventListener
    {
        private log4net.ILog _logger = log4net.LogManager.GetLogger("TermAppender");

        private class TermModel
        {
            public Guid TermId { get; set; }
            public TermState State { get; set; }
            public string Phrase { get; set; }
            public string PhraseLower { get; set; }
            public string BasePhrase { get; set; }
            public string Sentence { get; set; }
            public string Definition { get; set; }
            public short Box { get; set; }
            public DateTime? NextReview { get; set; }
            public Guid Text { get; set; }
            public Guid Language { get; set; }
            public DateTime Created { get; set; }
            public DateTime Modified { get; set; }
            public string[] Tags { get; set; }
            public short Length { get; set; }
            public Guid User { get; set; }
        }

        private void LogTerm(Term term)
        {
            if(term == null)
            {
                return;
            }

            var t = new TermModel
                {
                    BasePhrase = term.BasePhrase,
                    Box = term.Box,
                    Created = term.Created,
                    Definition = term.Definition,
                    Language = term.Language.LanguageId,
                    Length = term.Length,
                    Modified = term.Modified,
                    NextReview = term.NextReview,
                    Phrase = term.Phrase,
                    PhraseLower = term.PhraseLower,
                    Sentence = term.Sentence,
                    State = term.State,
                    TermId = term.TermId,
                    Text = term.Text == null ? Guid.Empty : term.Text.TextId,
                    User = term.User.UserId,
                    Tags = term.Tags.Select(x => x.TagTerm).ToArray()
                };

            var json = ServiceStack.Text.TypeSerializer.SerializeToString(t);
            _logger.Info(json);
        }

        public bool OnPreInsert(PreInsertEvent @event)
        {
            if(@event.Entity is Term)
            {
                LogTerm(@event.Entity as Term);
            }

            return false;
        }

        public bool OnPreUpdate(PreUpdateEvent @event)
        {
            if(@event.Entity is Term)
            {
                LogTerm(@event.Entity as Term);
            }

            return false;
        }

        public bool OnPreDelete(PreDeleteEvent @event)
        {
            if(@event.Entity is Term)
            {
                LogTerm(@event.Entity as Term);
            }

            return false;
        }
    }

    public sealed class EmptyStringModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            bindingContext.ModelMetadata.ConvertEmptyStringToNull = false;
            return base.BindModel(controllerContext, bindingContext);
        }
    }
}