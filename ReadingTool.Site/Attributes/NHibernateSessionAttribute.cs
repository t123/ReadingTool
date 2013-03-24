﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NHibernate;
using NHibernate.Context;

namespace ReadingTool.Site.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class NHibernateSessionAttribute : ActionFilterAttribute
    {
        public NHibernateSessionAttribute()
        {
            Order = 100;
        }

        protected ISessionFactory sessionFactory
        {
            get
            {
                return MvcApplication.SessionFactory;
            }
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = sessionFactory.OpenSession();
            CurrentSessionContext.Bind(session);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var session = CurrentSessionContext.Unbind(sessionFactory);
            session.Close();
        }
    }
}