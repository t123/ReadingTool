using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReadingTool.Entities;
using ServiceStack.OrmLite;

namespace ReadingTool.Services
{
    public interface ISequenceService
    {
        long Next();
    }

    public class SequenceService : ISequenceService
    {
        private readonly IDbConnection _db;

        public SequenceService(IDbConnection db)
        {
            _db = db;
        }

        public long Next()
        {
            var s = new Sequence();
            _db.Save<Sequence>(s);
            return s.Id;
        }
    }
}
