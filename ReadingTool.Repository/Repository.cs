using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Linq;

namespace ReadingTool.Repository
{
    public class Repository<T> : IRepository<T>
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected readonly ISessionFactory _sessionFactory;

        public ISession Session { get { return _session; } }

        protected ISession _session
        {
            get { return _sessionFactory.GetCurrentSession(); }
        }

        public Repository(ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        #region repository methods
        public virtual T LoadOne(object id)
        {
            return _session.Load<T>(id);
        }

        public virtual T FindOne(object id)
        {
            if(id == null)
            {
                return default(T);
            }

            return _session.Get<T>(id);
        }

        public virtual T FindOne(Expression<Func<T, bool>> exp)
        {
            return _session.Query<T>().FirstOrDefault(exp);
        }

        public virtual T Save(T entity, bool returnUpdated = false)
        {
            _session.SaveOrUpdate(entity);

            if(returnUpdated)
            {
                return entity;
            }

            return default(T);
        }

        public virtual void Save(IEnumerable<T> entities)
        {
            foreach(var entity in entities)
            {
                _session.SaveOrUpdate(entity);
            }
        }

        public virtual IQueryable<T> FindAll()
        {
            return from item in _session.Query<T>() select item;
        }

        public IQueryable<T> FindAll(Expression<Func<T, bool>> exp)
        {
            return _session.Query<T>().Where(exp);
        }

        public void DeleteAll(Expression<Func<T, bool>> exp)
        {
            var all = FindAll(exp);

            foreach(var a in all)
            {
                _session.Delete(a);
            }
        }

        public virtual void Delete(T entity)
        {
            _session.Delete(entity);
        }

        public virtual void Delete(object id)
        {
            Delete(FindOne(id));
        }
        #endregion
    }
}
