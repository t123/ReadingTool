#region License
// NeedsPersistenceAttribute.cs is part of ReadingTool.Api
// 
// ReadingTool.Api is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Api is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Api. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2013 Travis Watt
#endregion

using System;
using System.Web.Http;
using System.Web.Mvc;
using NHibernate;
using NHibernate.Context;
using Ninject;
using ReadingTool.Api.App_Start;

namespace ReadingTool.Api.Attributes
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
            var resolver = GlobalConfiguration.Configuration.DependencyResolver as NinjectDependencyResolver;
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