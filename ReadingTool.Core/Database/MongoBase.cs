using MongoDB.Driver;

namespace ReadingTool.Core.Database
{
    public class MongoBase
    {
        protected readonly MongoServer _server;
        protected readonly string _dbName;

        public MongoServer Server { get { return _server; } }
        public string DbName { get { return _dbName; } }

        public MongoBase(MongoServer server, string dbName)
        {
            _server = server;
            _dbName = dbName;
        }

        public virtual MongoDatabase Database
        {
            get { return Server.GetDatabase(_dbName); }
        }
    }
}