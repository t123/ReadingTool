#region License
// Repository.cs is part of ReadingTool.Repository
// 
// ReadingTool.Repository is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Repository is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Repository. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2013 Travis Watt
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Linq;

namespace ReadingTool.Repository
{
    public class Repository<T> : IRepository<T>
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //protected readonly ISessionFactory _sessionFactory;

        public ISession Session { get { return _session; } }

        protected ISession _session;
        //{
        //    get { return _sessionFactory.GetCurrentSession(); }
        //}

        //public Repository(ISessionFactory sessionFactory)
        //{
        //    _sessionFactory = sessionFactory;
        //}
        public Repository(ISession session)
        {
            _session = session;
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
