using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NHibernate;
using NHibernate.Context;
using Ninject;
using ReadingTool.Site.App_Start;

namespace ReadingTool.Site.Attributes
{
    public class NeedsPersistenceAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            BeginTransaction();
            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            EndTransaction(filterContext);
            CloseSession();
            base.OnActionExecuted(filterContext);
        }

        public void BeginTransaction()
        {
            var session = GetCurrentSession();
            if(session != null)
            {
                session.BeginTransaction();
            }
        }

        public void EndTransaction(ActionExecutedContext filterContext)
        {
            var session = GetCurrentSession();
            if(session != null)
            {
                if(session.Transaction.IsActive)
                {
                    if(filterContext.Exception == null)
                    {
                        session.Flush();
                        session.Transaction.Commit();
                    }
                    else
                    {
                        session.Transaction.Rollback();
                    }
                }
            }
        }

        private IKernel GetContainer()
        {
            var resolver = DependencyResolver.Current as NinjectDependencyResolver;
            if(resolver != null)
            {
                return resolver.Container;
            }

            throw new InvalidOperationException();
        }

        private ISession GetCurrentSession()
        {
            var container = GetContainer();
            var sessionFactory = container.Get<ISessionFactory>();
            var session = sessionFactory.GetCurrentSession();
            return session;
        }

        private void CloseSession()
        {
            var container = GetContainer();
            var sessionFactory = container.Get<ISessionFactory>();
            if(CurrentSessionContext.HasBind(sessionFactory))
            {
                var session = sessionFactory.GetCurrentSession();
                session.Close();
                session.Dispose();
                CurrentSessionContext.Unbind(sessionFactory);
            }
        }
    }
}