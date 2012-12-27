using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using ReadingTool.Core;
using ReadingTool.Entities;
using ServiceStack.OrmLite;

namespace ReadingTool.Services
{
    public interface ILanguageService
    {
        void Save(Language language);
        void Delete(Language language);
        void Delete(Guid id);
        Language Find(Guid id);
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
            if(language.Id == Guid.Empty)
            {
                language.Id = SequentialGuid.NewGuid();
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

        public void Delete(Guid id)
        {
            Delete(Find(id));
        }

        public Language Find(Guid id)
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
