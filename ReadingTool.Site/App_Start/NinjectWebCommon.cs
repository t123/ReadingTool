#region License
// NinjectWebCommon.cs is part of ReadingTool.Site
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
using System.Collections.Generic;
using System.Configuration;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Helpers;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using NHibernate;
using NHibernate.Context;
using NHibernate.Dialect;
using NHibernate.Event;
using NHibernate.Tool.hbm2ddl;
using Ninject;
using Ninject.Activation;
using Ninject.Web.Common;
using ReadingTool.Services;

[assembly: WebActivator.PreApplicationStartMethod(typeof(ReadingTool.Site.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(ReadingTool.Site.App_Start.NinjectWebCommon), "Stop")]

namespace ReadingTool.Site.App_Start
{
    public class NinjectControllerFactory : DefaultControllerFactory
    {
        private IKernel ninjectKernel;
        public NinjectControllerFactory(IKernel kernel)
        {
            ninjectKernel = kernel;
        }
        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            return (controllerType == null) ? null : (IController)ninjectKernel.Get(controllerType);
        }
    }

    public class NinjectDependencyResolver : IDependencyResolver
    {
        private readonly IKernel _container;
        public IKernel Container
        {
            get { return _container; }
        }
        public NinjectDependencyResolver(IKernel container)
        {
            _container = container;
        }

        public void Dispose()
        {
            // noop
        }

        public object GetService(Type serviceType)
        {
            return _container.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _container.GetAll(serviceType);
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

            ControllerBuilder.Current.SetControllerFactory(new NinjectControllerFactory(kernel));

            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            RegisterServices(kernel);

            DependencyResolver.SetResolver(new NinjectDependencyResolver(kernel));

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
            kernel.Bind<IEmailService>().To<EmailService>();
            kernel.Bind<IGroupService>().To<GroupService>();
            kernel.Bind<LwtImportService>().To<LwtImportService>();
            kernel.Bind<IPrincipal>().ToMethod(x => HttpContext.Current.User);
        }

        private static void InitNHibernate(IKernel kernel)
        {
            var cfg = Fluently.Configure()

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

#if DEBUG
            var dbSqlFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "readingtool.sql");
            cfg = cfg.ExposeConfiguration(config => new SchemaExport(config).SetOutputFile(dbSqlFile).Execute(true, false, false));
#endif

            switch(ConfigurationManager.AppSettings["dbType"])
            {
                case "sqlite":
                    cfg = cfg.Database(
                        SQLiteConfiguration
                            .Standard
                            .ConnectionString(ConfigurationManager.AppSettings["connectionString"])
                            .ShowSql()
                            .AdoNetBatchSize(200)
                        );
                    break;

                case "mysql":
                    cfg = cfg.Database(
                        MySQLConfiguration
                            .Standard
                            .ConnectionString(ConfigurationManager.AppSettings["connectionString"])
                            .ShowSql()
                            .AdoNetBatchSize(200)
                        );
                    break;

                case "mssql2005":
                    cfg = cfg.Database(
                        MsSqlConfiguration
                            .MsSql2005
                            .ConnectionString(ConfigurationManager.AppSettings["connectionString"])
                            .ShowSql()
                            .AdoNetBatchSize(200)
                        );
                    break;

                case "mssql2008":
                default:
                    cfg = cfg.Database(
                        MsSqlConfiguration
                            .MsSql2008
                            .ConnectionString(ConfigurationManager.AppSettings["connectionString"])
                            .ShowSql()
                            .AdoNetBatchSize(200)
                        );
                    break;
            }

            var sessionFactory = cfg.BuildSessionFactory();
            kernel.Bind<ISessionFactory>().ToConstant(sessionFactory);
            kernel.Bind<ISession>().ToMethod(CreateSession);
        }

        private static ISession CreateSession(IContext context)
        {
            var sessionFactory = context.Kernel.Get<ISessionFactory>();
            if(!CurrentSessionContext.HasBind(sessionFactory))
            {
                var session = sessionFactory.OpenSession();
                CurrentSessionContext.Bind(session);
            }
            return sessionFactory.GetCurrentSession();
        }
    }
}
