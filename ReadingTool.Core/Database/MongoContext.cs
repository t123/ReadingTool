using System;
using System.Configuration;
using MongoDB.Driver;

namespace ReadingTool.Core.Database
{
    public sealed class MongoContext : MongoBase
    {
        public MongoContext(MongoServer server, string dbName)
            : base(server, dbName)
        {
        }

        public override MongoDatabase Database
        {
            get { return Server.GetDatabase(DbName); }
        }

        private static readonly Lazy<MongoContext> _lazy = new Lazy<MongoContext>(
            () => RegisterMongoDb(
                ConfigurationManager.ConnectionStrings["default"].ConnectionString,
                ConfigurationManager.AppSettings["DbName"]
                      ));

        public static MongoContext Instance { get { return _lazy.Value; } }

        private static MongoContext RegisterMongoDb(string connectionString, string dbName)
        {
            MongoClient mc = new MongoClient(connectionString);
            var readServer = mc.GetServer();
            var read = new MongoContext(readServer, dbName);

            return read;
        }
    }
}
