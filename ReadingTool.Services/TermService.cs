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

        public Tuple<Term[], Term[]> FindAllForParsing(Language language)
        {
            return new Tuple<Term[], Term[]>(new Term[] { }, new Term[] { });
        }
    }
}
