using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Ninject;
using Ninject.Web.Common;
using ReadingTool.Entities;
using ReadingTool.Services;
using ReadingTool.Site.Mappings;
using ServiceStack.DataAccess;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.SqlServer;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

namespace ReadingTool.Site
{
    internal static class ContextPerRequest
    {
        private static readonly OrmLiteConnectionFactory _factory = DependencyResolver.Current.GetService<OrmLiteConnectionFactory>();

        internal static IDbConnection Current
        {
            get
            {
                if(!HttpContext.Current.Items.Contains("myContext"))
                {
                    OrmLiteConfig.DialectProvider.UseUnicode = true;
                    HttpContext.Current.Items.Add("myContext", _factory.OpenDbConnection());
                }

                return HttpContext.Current.Items["myContext"] as IDbConnection;
            }
        }
    }

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : HttpApplication
    {
        //public static readonly OrmLiteConnectionFactory dbFactory = new OrmLiteConnectionFactory("Data Source=(local);Initial Catalog=Test;Persist Security Info=True;User ID=sa;Password=password", SqlServerOrmLiteDialectProvider.Instance)
        //    {
        //        ConnectionFilter = x => new ProfiledDbConnection((DbConnection)((IHasDbConnection)x).DbConnection, MiniProfiler.Current)
        //    };

        //public static readonly OrmLiteConnectionFactory dbFactory = new OrmLiteConnectionFactory(":memory:", false, SqliteDialect.Provider);

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            ModelBinders.Binders.DefaultBinder = new EmptyStringModelBinder();

            RegisterMappings.Register();

            var connection = ContextPerRequest.Current;
            //connection.DropAndCreateTable<Text>();
            //connection.DropAndCreateTable<SystemLanguage>();
            //connection.DropAndCreateTable<Language>();
            //connection.DropAndCreateTable<LanguageSettings>();
            //connection.DropAndCreateTables(typeof(User));
        }

        private void Application_BeginRequest()
        {
        }

        private void Application_EndRequest()
        {
            var connection = HttpContext.Current.Items["myContext"] as IDbConnection;

            if(connection != null)
            {
                connection.Dispose();
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

            if(authCookie != null)
            {
                var authenticationService = DependencyResolver.Current.GetService<IAuthenticationService>();

                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                var userData = authenticationService.GetUserData(authTicket.UserData);

                IIdentity identity = HttpContext.Current.User.Identity;
                UserPrincipal newUser = authenticationService.ConstructUserPrincipal(
                    identity,
                    userData[AuthenticationService.DISPLAYNAME],
                    userData[AuthenticationService.ROLES]
                    );

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