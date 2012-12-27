using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using ReadingTool.Entities;
using ServiceStack.OrmLite;

namespace ReadingTool.Services
{
    public interface ISystemLanguageService
    {
        void Save(SystemLanguage language);
        void Save(IList<SystemLanguage> languages);
        void Delete(SystemLanguage language);
        void Delete(long id);
        SystemLanguage Find(long id);
        IEnumerable<SystemLanguage> FindAll();
        IEnumerable<SystemLanguage> FindAllStartingWith(string term);
        SystemLanguage FindByName(string name);
    }

    public class SystemLanguageService : ISystemLanguageService
    {
        private readonly IDbConnection _db;

        public SystemLanguageService(IDbConnection db)
        {
            _db = db;
        }

        public void Save(SystemLanguage language)
        {
            _db.Save(language);
        }

        public void Save(IList<SystemLanguage> languages)
        {
            _db.SaveAll(languages);
        }

        public void Delete(SystemLanguage language)
        {
            throw new NotImplementedException();
        }

        public void Delete(long id)
        {
            throw new NotImplementedException();
        }

        public SystemLanguage Find(long id)
        {
            return _db.GetById<SystemLanguage>(id);
        }

        public IEnumerable<SystemLanguage> FindAll()
        {
            return _db.Select<SystemLanguage>();
        }

        public IEnumerable<SystemLanguage> FindAllStartingWith(string term)
        {
            return _db.Select<SystemLanguage>(x => x.Name.StartsWith(term));
        }

        public SystemLanguage FindByName(string name)
        {
            return _db.Select<SystemLanguage>(x => x.Name==name).FirstOrDefault();
        }
    }
}
