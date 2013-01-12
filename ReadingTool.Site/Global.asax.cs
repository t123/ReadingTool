using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Elmah;
using MongoDB.Bson;
using Ninject;
using Ninject.Web.Common;
using ReadingTool.Entities;
using ReadingTool.Services;
using ReadingTool.Site.Mappings;

namespace ReadingTool.Site
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : HttpApplication
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //public static readonly OrmLiteConnectionFactory dbFactory = new OrmLiteConnectionFactory("Data Source=(local);Initial Catalog=Test;Persist Security Info=True;User ID=sa;Password=password", SqlServerOrmLiteDialectProvider.Instance)
        //    {
        //        ConnectionFilter = x => new ProfiledDbConnection((DbConnection)((IHasDbConnection)x).DbConnection, MiniProfiler.Current)
        //    };

        //public static readonly OrmLiteConnectionFactory dbFactory = new OrmLiteConnectionFactory(":memory:", false, SqliteDialect.Provider);

        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();
            Logger.Debug("Application start");

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            ModelBinders.Binders.DefaultBinder = new EmptyStringModelBinder();
            ModelBinders.Binders.Add(typeof(ObjectId), new ObjectIdBinder());

            RegisterMappings.Register();

            ReadingTool.Site.Init.Start();
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
                    string[] ignoreExtensions = new string[] { ".php", ".asp", ".aspx", ".gif", ".png", ".ico" };
                    if(ignoreExtensions.Select(url.EndsWith).Any())
                    {
                        e.Dismiss();
                    }
                    else if(url.StartsWith("/apple-touch"))
                    {
                        e.Dismiss();
                    }
                }
            }
        }

        private void Application_BeginRequest()
        {
        }

        private void Application_EndRequest()
        {
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

            if(authCookie != null)
            {
                var authenticationService = DependencyResolver.Current.GetService<IAuthenticationService>();

                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                IIdentity identity = HttpContext.Current.User.Identity;
                UserPrincipal newUser = authenticationService.ConstructUserPrincipal(identity, authTicket.UserData);
                Context.User = newUser;
                System.Threading.Thread.CurrentPrincipal = HttpContext.Current.User;
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
}