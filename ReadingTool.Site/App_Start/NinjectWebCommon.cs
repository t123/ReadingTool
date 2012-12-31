using System.Data;
using System.Data.Common;
using System.Security.Principal;
using ReadingTool.Entities;
using ReadingTool.Services;
using ServiceStack.DataAccess;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.SqlServer;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

[assembly: WebActivator.PreApplicationStartMethod(typeof(ReadingTool.Site.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(ReadingTool.Site.App_Start.NinjectWebCommon), "Stop")]

namespace ReadingTool.Site.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;

    public static class NinjectWebCommon
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            RegisterServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IParserService>().To<DefaultParserService>();
            kernel.Bind<ITermService>().To<TermService>();
            kernel.Bind<IUserService>().To<UserService>();
            kernel.Bind<IAuthenticationService>().To<AuthenticationService>();
            kernel.Bind<ILanguageService>().To<LanguageService>();
            kernel.Bind<ITextService>().To<TextService>();
            kernel.Bind<ISystemLanguageService>().To<SystemLanguageService>();
            kernel.Bind<IDeleteService>().To<DeleteService>();
            kernel.Bind<IDbConnection>().ToMethod(x => ContextPerRequest.Current);
            kernel.Bind<IPrincipal>().ToMethod(x => HttpContext.Current.User);
            kernel.Bind<ISequenceService>().To<SequenceService>();
            kernel.Bind<ILwtImportService>().To<LwtImportService>();

            kernel.Bind<OrmLiteConnectionFactory>().ToMethod
                (
                    x =>
                    new OrmLiteConnectionFactory("Data Source=(local);Initial Catalog=Test;Persist Security Info=True;User ID=sa;Password=password", SqlServerOrmLiteDialectProvider.Instance)
                        {
                            ConnectionFilter = z => new ProfiledDbConnection((DbConnection)((IHasDbConnection)z).DbConnection, MiniProfiler.Current)
                        }
                ).InSingletonScope();
        }
    }
}
