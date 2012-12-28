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
    public interface IDeleteService
    {
        void DeleteUser(User user);
        void DeleteLanguage(Language language);
        void DeleteText(Text text);
        void DeleteTerm(Term term);
    }

    public class DeleteService : IDeleteService
    {
        //TODO change to DELETE * WHERE x=x <-- allow IEnumerables

        private readonly IDbConnection _db;
        private readonly IUserIdentity _identity;

        public DeleteService(IDbConnection db, IPrincipal principal)
        {
            _db = db;
            _identity = principal.Identity as IUserIdentity;
        }

        public void DeleteUser(User user)
        {
            if(user == null || user.Id != _identity.UserId)
            {
                return;
            }

            _db.Select<Language>(x => x.Owner == user.Id).ForEach(DeleteLanguage);
            _db.DeleteById<User>(user.Id);
        }

        public void DeleteLanguage(Language language)
        {
            if(language == null || language.Owner != _identity.UserId)
            {
                return;
            }

            _db.Select<Text>(x => x.L1Id == language.Id).ForEach(DeleteText);
            _db.Select<Term>(x => x.LanguageId == language.Id).ForEach(DeleteTerm);
            _db.DeleteById<Language>(language.Id);
        }

        public void DeleteTerm(Term term)
        {
            if(term == null || term.Id != _identity.UserId)
            {
                return;
            }

            _db.Delete<Tag>(x => x.TermId == term.Id);
            _db.DeleteById<Text>(term.Id);
        }

        public void DeleteText(Text text)
        {
            if(text == null || text.Owner != _identity.UserId)
            {
                return;
            }

            _db.Delete<Tag>(x => x.TextId == text.Id);
            _db.DeleteById<Text>(text.Id);
        }

        //public void DeleteTexts(IEnumerable<Text> texts)
        //{
        //    if(texts == null) return;
        //    var toDelete = texts.Where(x => x.Owner == _identity.UserId);
        //    var ids = toDelete.Select(x => x.Id).ToList();

        //    _db.Delete<Tag>("TextId IN ({0})", ids);
        //    _db.DeleteByIds<Text>(ids);
        //}
    }
}
