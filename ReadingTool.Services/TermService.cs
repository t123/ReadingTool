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
    public interface ITermService
    {
        void Save(Term term);
        void Delete(Term term);
        void Delete(Guid id);
        Term Find(Guid id);
        Term Find(Guid languageId, string term);
        IEnumerable<Term> FindAll();
        Tuple<Term[], Term[]> FindAllForParsing(Language language);
    }

    public class TermService : ITermService
    {
        private readonly IDbConnection _db;
        private readonly IUserIdentity _identity;

        public TermService(IDbConnection db, IPrincipal principal)
        {
            _db = db;
            _identity = principal.Identity as IUserIdentity;
        }

        #region basic
        public void Save(Term term)
        {
            throw new NotImplementedException();
        }

        public void Delete(Term term)
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public Term Find(Guid id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Term> FindAll()
        {
            throw new NotImplementedException();
        }

        public bool Exists(Guid languageId, string termPhrase)
        {
            return _db.Select<Term>(x => x.LanguageId == languageId && x.TermPhrase == termPhrase).FirstOrDefault() != null;
        }
        #endregion

        #region parsing
        public Tuple<Term[], Term[]> FindAllForParsing(Language language)
        {
            return new Tuple<Term[], Term[]>(new Term[] { }, new Term[] { });
        }
        #endregion

        #region reading
        public Term Find(Guid languageId, string termPhrase)
        {
            Term term = _db.Select<Term>(x => x.TermPhrase == termPhrase && x.LanguageId == languageId && x.Owner == _identity.UserId).FirstOrDefault();

            if(term == null) return null;

            var individual = _db.Select<IndividualTerm>(x => x.TermId == term.Id);
            term.AddIndividualTerms(individual);

            return term;
        }

        #endregion
    }
}
