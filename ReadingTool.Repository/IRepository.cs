using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ReadingTool.Repository
{
    public interface IRepository<T>
    {
        T LoadOne(object id);
        T FindOne(object id);
        T FindOne(Expression<Func<T, bool>> exp);
        T Save(T entity, bool returnUpdated = false);
        void Save(IEnumerable<T> entities);
        IQueryable<T> FindAll();
        IQueryable<T> FindAll(Expression<Func<T, bool>> exp);
        void Delete(T entity);
        void Delete(object id);
        void DeleteAll(Expression<Func<T, bool>> exp);
    }
}
