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
    public interface ILanguageService
    {
        void Save(Language language);
        void Delete(Language language);
        void Delete(long id);
        Language Find(long id);
        IEnumerable<Language> FindAll();
        Language FindByName(string name);
    }

    public class LanguageService : ILanguageService
    {
        private readonly IDbConnection _db;
        private readonly IUserIdentity _identity;

        public LanguageService(IDbConnection db, IPrincipal principal)
        {
            _db = db;
            _identity = principal.Identity as IUserIdentity;
        }

        public void Save(Language language)
        {
            if(language.Id == 0)
            {
                language.Created = DateTime.Now;
                language.Owner = _identity.UserId;
            }

            language.Modified = DateTime.Now;

            _db.Save(language);
        }

        public void Delete(Language language)
        {
            if(language == null)
            {
                return;
            }

            _db.DeleteById<Language>(language.Id);
        }

        public void Delete(long id)
        {
            Delete(Find(id));
        }

        public Language Find(long id)
        {
            return _db.Select<Language>(x => x.Id == id && x.Owner == _identity.UserId).FirstOrDefault();
        }

        public IEnumerable<Language> FindAll()
        {
            return _db.Select<Language>(x => x.Owner == _identity.UserId);
        }

        public Language FindByName(string name)
        {
            return _db.Select<Language>(x => x.Name == name).FirstOrDefault();
        }
    }
}
