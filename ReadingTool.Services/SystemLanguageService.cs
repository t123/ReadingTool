using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using ReadingTool.Core.Database;
using ReadingTool.Entities;
using ReadingTool.Repository;

namespace ReadingTool.Services
{
    public interface ISystemLanguageService : IRepository<SystemLanguage>
    {
        IEnumerable<SystemLanguage> FindAllStartingWith(string term);
        SystemLanguage FindByName(string name);
    }

    public class SystemLanguageService : Repository<SystemLanguage>, ISystemLanguageService
    {
        public SystemLanguageService(MongoContext context)
            : base(context)
        {
        }

        public new void Delete(SystemLanguage language)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SystemLanguage> FindAllStartingWith(string term)
        {
            return Queryable.Where(x => x.Name.StartsWith(term));
        }

        public SystemLanguage FindByName(string name)
        {
            return Queryable.FirstOrDefault(x => x.Name == name);
        }
    }
}
