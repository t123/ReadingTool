﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Context;
using NHibernate.Event;
using NHibernate.Tool.hbm2ddl;
using ReadingTool.Common;
using ReadingTool.Entities;
using ReadingTool.Site.Models.Terms;

namespace ReadingTool.Site
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        private log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            ModelBinders.Binders.DefaultBinder = new EmptyStringModelBinder();

            Mappings.Register();
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
            public long TermId { get; set; }
            public TermState State { get; set; }
            public string Phrase { get; set; }
            public string PhraseLower { get; set; }
            public string BasePhrase { get; set; }
            public string Sentence { get; set; }
            public string Definition { get; set; }
            public short Box { get; set; }
            public DateTime? NextReview { get; set; }
            public long Text { get; set; }
            public long Language { get; set; }
            public DateTime Created { get; set; }
            public DateTime Modified { get; set; }
            public string[] Tags { get; set; }
            public short Length { get; set; }
            public long User { get; set; }
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
                    Text = term.Text == null ? 0 : term.Text.TextId,
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