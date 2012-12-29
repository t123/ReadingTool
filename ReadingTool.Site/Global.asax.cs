using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
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
            //connection.DropAndCreateTable<SystemLanguage>();

            if(connection.TableExists("LanguageSettings"))
            {
                connection.DeleteAll<LanguageSettings>();
                connection.DropTable<LanguageSettings>();
            }

            if(connection.TableExists("Tag"))
            {
                connection.DeleteAll<Tag>();
                connection.DropTable<Tag>();
            }

            if(connection.TableExists("Text"))
            {
                connection.DeleteAll<Text>();
                connection.DropTable<Text>();
            }

            if(connection.TableExists("Language"))
            {
                connection.DeleteAll<Language>();
                connection.DropTable<Language>();
            }

            if(connection.TableExists("Term"))
            {
                connection.DeleteAll<Term>();
                connection.DropTable<Term>();
            }

            if(connection.TableExists("IndividualTerm"))
            {
                connection.DeleteAll<IndividualTerm>();
                connection.DropTable<IndividualTerm>();
            }

            if(connection.TableExists("User"))
            {
                connection.DeleteAll<User>();
                connection.DropTable<User>();
            }

            if(connection.TableExists("Sequence"))
            {
                connection.DeleteAll<Sequence>();
                connection.DropTable<Sequence>();
            }

            connection.CreateTable<User>(true);
            connection.CreateTable<Language>(true);
            connection.CreateTable<Text>(true);
            connection.CreateTable<Term>(true);
            connection.CreateTable<IndividualTerm>(true);
            connection.CreateTable<Tag>(true);
            connection.CreateTable<Sequence>(true);

            using(StreamReader sr = new StreamReader(Path.Combine(Server.MapPath("~/App_Data"), "dummy.sql"), Encoding.UTF8))
            {
                while(!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if(string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    connection.ExecuteSql(line);
                }
            }
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