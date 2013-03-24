using System;
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
        public static ISessionFactory SessionFactory { get; private set; }

        private void InitNHibernate()
        {
            var cfg = Fluently.Configure()
                .Database(
                    MsSqlConfiguration
                        .MsSql2008
                        .ConnectionString(ConfigurationSettings.AppSettings["connectionString"])
                        .ShowSql()
                        .AdoNetBatchSize(200)
                )
                .CurrentSessionContext<WebSessionContext>()
                .Cache(x => x.UseQueryCache())
                .Cache(x => x.UseSecondLevelCache())
                .Cache(x => x.ProviderClass("NHibernate.Caches.SysCache2.SysCacheProvider, NHibernate.Caches.SysCache2"))
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<ReadingTool.Entities.User>())
                .ExposeConfiguration(config => new SchemaUpdate(config).Execute(false, true))
                .ExposeConfiguration(x =>
                {
                    x.EventListeners.PreInsertEventListeners = new IPreInsertEventListener[] { new AuditEventListener() };
                    x.EventListeners.PreUpdateEventListeners = new IPreUpdateEventListener[] { new AuditEventListener() };
                }
                )
                ;

            SessionFactory = cfg.BuildSessionFactory();
        }

        protected void Application_Start()
        {
            InitNHibernate();

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

    internal class AuditEventListener : IPreInsertEventListener, IPreUpdateEventListener
    {
        private log4net.ILog _logger = log4net.LogManager.GetLogger("TermAppender");

        class TermModel
        {
            public Guid TermId { get; set; }
            public TermState State { get; set; }
            public string Phrase { get; set; }
            public string BasePhrase { get; set; }
            public string Sentence { get; set; }
            public string Definition { get; set; }
            public short Box { get; set; }
            public virtual DateTime? NextReview { get; set; }
            public virtual Guid Text { get; set; }
            public virtual Guid Language { get; set; }
            public virtual DateTime Created { get; set; }
            public virtual DateTime Modified { get; set; }
            public virtual string[] Tags { get; set; }
            public virtual short Length { get; set; }
            public virtual Guid User { get; set; }
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