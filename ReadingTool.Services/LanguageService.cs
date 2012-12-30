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
        IEnumerable<Language> FindAllIncludePublic();
        Language FindByName(string name);
    }

    public class LanguageService : ILanguageService
    {
        private readonly IDbConnection _db;
        private readonly IDeleteService _deleteService;
        private readonly IUserIdentity _identity;

        public LanguageService(IDbConnection db, IPrincipal principal, IDeleteService deleteService)
        {
            _db = db;
            _deleteService = deleteService;
            _identity = principal.Identity as IUserIdentity;
        }

        public void Save(Language language)
        {
            if(language.IsPublic && !_identity.IsInRole(Constants.Roles.ADMIN))
            {
                return;
            }

            if(language.Id == Guid.Empty)
            {
                language.Id = SequentialGuid.NewGuid();
                language.Created = DateTime.Now;
                language.Owner = language.IsPublic ? User.DummyOwner : _identity.UserId;
            }

            language.Modified = DateTime.Now;

            _db.Save(language);
        }

        public void Delete(Language language)
        {
            if(language.IsPublic && !_identity.IsInRole(Constants.Roles.ADMIN))
            {
                return;
            }

            _deleteService.DeleteLanguage(language);
        }

        public void Delete(Guid id)
        {
            Delete(Find(id));
        }

        public Language Find(Guid id)
        {
            var language = _db.Select<Language>(x => x.Id == id).FirstOrDefault();

            if(language == null)
            {
                return null;
            }

            if(
                (language.Owner == _identity.UserId) ||
                (language.IsPublic && _identity.IsInRole(Constants.Roles.ADMIN))
                )
            {
                return language;
            }

            return null;
        }

        public IEnumerable<Language> FindAll()
        {
            return _db.Select<Language>(x => x.Owner == _identity.UserId);
        }

        public IEnumerable<Language> FindAllIncludePublic()
        {
            return _db.Select<Language>(x => x.Owner == _identity.UserId || x.IsPublic);
        }

        public Language FindByName(string name)
        {
            return _db.Select<Language>(x => x.Name == name).FirstOrDefault();
        }
    }
}
