using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using ReadingTool.Core.Database;

namespace ReadingTool.Repository
{
    public interface IRepository<T> where T : IEntity
    {
        string CollectionName { get; }
        MongoCollection<T> Collection { get; }
        T FindOne(ObjectId id);
        void Save(T entity);
        void Save(IEnumerable<T> entities);
        IEnumerable<T> FindAll();
        void Delete(T entity);
        void Delete(ObjectId id);
    }

    public class Repository<T> : IRepository<T> where T : IEntity
    {
        protected readonly MongoContext _mongoContext;
        protected readonly string _collectionName = typeof(T).Name;
        protected readonly MongoCollection<T> _collection;

        public string CollectionName
        {
            get { return _collectionName; }
        }

        public MongoCollection<T> Collection
        {
            get { return _collection; }
        }

        public Repository(MongoContext mongoContext)
        {
            _mongoContext = mongoContext;
            _collection = _mongoContext.Database.GetCollection<T>(_collectionName);
        }

        public virtual T FindOne(ObjectId id)
        {
            return _collection.FindOneById(id);
        }

        public virtual void Save(T entity)
        {
            _collection.Save(entity);
        }

        public virtual void Save(IEnumerable<T> entities)
        {
            _collection.Save(entities);
        }

        public virtual IEnumerable<T> FindAll()
        {
            return _collection.FindAll();
        }

        public virtual void Delete(T entity)
        {
            _collection.Remove(Query.EQ("_id", entity.Id));
        }

        public virtual void Delete(ObjectId id)
        {
            Delete(FindOne(id));
        }
    }
}
