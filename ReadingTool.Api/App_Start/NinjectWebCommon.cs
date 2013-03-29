using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Security.Principal;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Routing;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Context;
using NHibernate.Tool.hbm2ddl;
using Ninject.Activation;
using Ninject.Syntax;
using ReadingTool.Services;
using IDependencyResolver = System.Web.Mvc.IDependencyResolver;

[assembly: WebActivator.PreApplicationStartMethod(typeof(ReadingTool.Api.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(ReadingTool.Api.App_Start.NinjectWebCommon), "Stop")]

namespace ReadingTool.Api.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;

    public class NinjectDependencyScope : IDependencyScope
    {
        private IResolutionRoot resolver;

        internal NinjectDependencyScope(IResolutionRoot resolver)
        {
            Contract.Assert(resolver != null);

            this.resolver = resolver;
        }

        public void Dispose()
        {
            IDisposable disposable = resolver as IDisposable;
            if(disposable != null)
                disposable.Dispose();

            resolver = null;
        }

        public object GetService(Type serviceType)
        {
            if(resolver == null)
                throw new ObjectDisposedException("this", "This scope has already been disposed");

            return resolver.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            if(resolver == null)
                throw new ObjectDisposedException("this", "This scope has already been disposed");

            return resolver.GetAll(serviceType);
        }
    }

    public class NinjectDependencyResolver : NinjectDependencyScope, System.Web.Http.Dependencies.IDependencyResolver
    {
        private IKernel kernel;
        public IKernel Container
        {
            get { return kernel; }
        }
        public NinjectDependencyResolver(IKernel kernel)
            : base(kernel)
        {
            this.kernel = kernel;
        }

        public IDependencyScope BeginScope()
        {
            return new NinjectDependencyScope(kernel.BeginBlock());
        }
    }

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

            GlobalConfiguration.Configuration.DependencyResolver = new NinjectDependencyResolver(kernel);

            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            InitNHibernate(kernel);

            kernel.Bind<IUserService>().To<UserService>();
            kernel.Bind<ITextService>().To<TextService>();
            kernel.Bind<IParserService>().To<DefaultParserService>();
            kernel.Bind<IPrincipal>().ToMethod(x => HttpContext.Current.User);
        }

        private static void InitNHibernate(IKernel kernel)
        {
            var cfg = Fluently.Configure()
                .Database(
                    MsSqlConfiguration
                        .MsSql2008
                        .ConnectionString(ConfigurationManager.AppSettings["connectionString"])
                        .ShowSql()
                        .AdoNetBatchSize(200)
                )
                .CurrentSessionContext<WebSessionContext>()
                .Cache(x => x.UseQueryCache())
                .Cache(x => x.UseSecondLevelCache())
                .Cache(x => x.ProviderClass("NHibernate.Caches.SysCache2.SysCacheProvider, NHibernate.Caches.SysCache2"))
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<ReadingTool.Entities.User>())
                ;

            var sessionFactory = cfg.BuildSessionFactory();
            kernel.Bind<ISessionFactory>().ToConstant(sessionFactory);
            kernel.Bind<ISession>().ToMethod(CreateSession);
        }

        private static ISession CreateSession(IContext context)
        {
            var sessionFactory = context.Kernel.Get<ISessionFactory>();
            if(!CurrentSessionContext.HasBind(sessionFactory))
            {
                // Open new ISession and bind it to the current session context
                var session = sessionFactory.OpenSession();
                CurrentSessionContext.Bind(session);
            }
            return sessionFactory.GetCurrentSession();
        }
    }
}
