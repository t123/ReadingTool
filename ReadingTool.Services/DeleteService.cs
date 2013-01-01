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

            _db.Select<Term>(x => x.LanguageId == language.Id).ForEach(DeleteTerm);
            _db.Select<Text>(x => x.L1Id == language.Id).ForEach(DeleteText);
            _db.Update<Text>(new { L2Id = (Guid?)null }, x => x.L2Id == language.Id);
            _db.DeleteById<Language>(language.Id);
        }

        public void DeleteTerm(Term term)
        {
            if(term == null || term.Owner != _identity.UserId)
            {
                return;
            }

            var it = _db.Select<IndividualTerm>(x => x.TermId == term.Id);

            foreach(var i in it)
            {
                _db.Delete<Tag>(x => x.TermId == i.Id);
            }

            _db.Delete<IndividualTerm>(x => x.TermId == term.Id);
            _db.DeleteById<Term>(term.Id);
        }

        public void DeleteText(Text text)
        {
            if(text == null || text.Owner != _identity.UserId)
            {
                return;
            }

            _db.Update<IndividualTerm>(new { TextId = (Guid?)null }, x => x.TextId == text.Id);
            _db.Delete<Tag>(x => x.TextId == text.Id);
            _db.DeleteById<Text>(text.Id);
        }
    }
}
