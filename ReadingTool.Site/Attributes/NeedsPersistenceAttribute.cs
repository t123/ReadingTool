using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NHibernate;

namespace ReadingTool.Site.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class NeedsPersistenceAttribute : NHibernateSessionAttribute
    {
        protected ISession session
        {
            get
            {
                return sessionFactory.GetCurrentSession();
            }
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            session.BeginTransaction();
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var tx = session.Transaction;

            if(tx != null && tx.IsActive)
            {
                if(filterContext.Exception != null && !filterContext.ExceptionHandled)
                {
                    session.Transaction.Rollback();
                }
                else
                {
                    try
                    {
                        session.Transaction.Commit();
                    }
                    catch(SqlException e)
                    {
                        session.Transaction.Rollback();
                    }
                }
            }
            base.OnActionExecuted(filterContext);
        }
    }
}